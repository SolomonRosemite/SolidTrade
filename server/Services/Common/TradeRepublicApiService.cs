using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OneOf;
using Serilog;
using SolidTradeServer.Common;
using SolidTradeServer.Data.Dtos.Common;
using SolidTradeServer.Data.Models.Enums;
using SolidTradeServer.Data.Models.Errors;
using WebSocketSharp;
using Timer = System.Timers.Timer;

namespace SolidTradeServer.Services.Common
{
    public class TradeRepublicApiService
    {
        private readonly ILogger _logger = Log.ForContext<TradeRepublicApiService>();
        private readonly Dictionary<int, Action<string>> _runningRequests = new();
        private readonly Dictionary<int, Action<string>> _runningRequestsAsync = new();
        private readonly WebSocket _webSocket;

        private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
        private readonly Timer _timer;
        private int _latestId;
        
        // Todo: On application start add all the ongoing orders.
        public TradeRepublicApiService(IConfiguration configuration)
        {
            _timer = new Timer(1000 * 30);
            
            InitTimer();
            _webSocket = new WebSocket(configuration["TradeRepublic:ApiEndpoint"]);
            _webSocket.OnOpen += (_, _) =>
            {
                _webSocket.Send(configuration["TradeRepublic:InitialConnectString"]);
                _webSocket.OnMessage += OnTradeRepublicMessage;
            };

            _webSocket.OnClose += (_, _) =>
            {
                _logger.Fatal("Trade republic stock connection closed unexpectedly.");
            };
            
            _webSocket.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
            _webSocket.Connect();
        }

        public async Task<OneOf<T, UnexpectedError>> AddRequest<T>(string content, CancellationToken token)
        {
            var tcs = new TaskCompletionSource<OneOf<T, UnexpectedError>>();
            var id = GetNewId();
            
            _runningRequests.Add(id, response =>
            {
                var res = ConvertToObject<T>(response);
                tcs.SetResult(res);
            });

            _webSocket.Send($"sub {id} {content}");

            while (!tcs.Task.IsCompleted)
                await Task.Delay(50, token);
            
            return await tcs.Task;
        }

        public void AddRequest<T>(string content, Func<T, OngoingTradeResponse> action)
        {
            var id = GetNewId();
            
            _runningRequestsAsync.Add(id, response =>
            {
                // throw new Exception();
                if (ConvertToObject<T>(response).TryPickT0(out var value, out var err))
                {
                    var result = action.Invoke(value);

                    if (result is OngoingTradeResponse.Complete)
                    {
                        _webSocket.Send($"unsub {id}");
                        _runningRequestsAsync.Remove(id);
                    }
                }
                else
                {
                    _logger.Error(Constants.LogMessageTemplate, err);
                    
                    _webSocket.Send($"unsub {id}");
                    _runningRequestsAsync.Remove(id);
                }
            });

            _webSocket.Send($"sub {id} {content}");
        }

        public async Task<OneOf<bool, UnexpectedError>> IsStockMarketOpen(string isin)
        {
            var tcs = new TaskCompletionSource<OneOf<bool, UnexpectedError>>();
            var id = GetNewId();
            
            _runningRequests.Add(id, response =>
            {
                tcs.SetResult(ConvertToObject<TradeRepublicIsStockMarketOpenResponseDto>(response)
                    .Match<OneOf<bool, UnexpectedError>>(dto => !dto.Open.HasValue || dto.Open.Value, error => error));
            });

            string content = "{\"type\":\"aggregateHistoryLight\",\"range\":\"1d\",\"id\":\""+ isin + "\"}";
            _webSocket.Send($"sub {id} {content}");

            return await tcs.Task;
        }

        private void OnTradeRepublicMessage(object? sender, MessageEventArgs e)
        {
            _logger.Information(Constants.LogMessageTemplate, new TradeRepublicMessage
            {
                Title = "Trade Republic api message",
                Message = e.Data,
            });
            
            if (e.Data == "connected" || e.Data.StartsWith("echo"))
                return;

            var id = ParseId(e.Data);

            if (id == -1)
                return;

            var message = GetMessageResponse(e.Data);

            if (_runningRequests.ContainsKey(id))
            {
                _webSocket.Send($"unsub {id}");

                _runningRequests[id].Invoke(message);
                _runningRequests.Remove(id);
            } else if (_runningRequestsAsync.ContainsKey(id))
            {
                _runningRequestsAsync[id].Invoke(message);
            }
        }

        private string GetMessageResponse(string messageInput)
        {
            int index = messageInput.IndexOf('{') - 1;

            return index < 0 ? null : messageInput[index..];
        }
        
        private int ParseId(string message)
        {
            try
            {
                return int.Parse(message[..message.IndexOf(' ')]);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.LogMessageTemplate, new UnexpectedError
                {
                  Title  = "Failed to parse id",
                  Message = "Failed to parse id from trade republic message.",
                  Exception = e,
                  AdditionalData = new { message }
                });
                return -1;
            }
        }

        private void InitTimer()
        {
            _timer.AutoReset = true;
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }

        private void TimerOnElapsed(object sender, EventArgs e)
        {
            if (_webSocket.IsAlive)
                _webSocket.Send("echo " + DateTime.Now.Ticks);
        }
        
        private OneOf<T, UnexpectedError> ConvertToObject<T>(string content)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(content, _jsonSerializerOptions);
            }
            catch (Exception e)
            {
                return new UnexpectedError
                {
                    Title = "Json parsing error",
                    Message = "Paring Trade Republic message response failed.",
                    Exception = e,
                    AdditionalData = new { Response = content, Type = typeof(T) },
                };
            }
        }

        private int GetNewId()
        {
            return ++_latestId;
        }
     }
}