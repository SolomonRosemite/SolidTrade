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
using SolidTradeServer.Data.Dtos.Warrant.Response;
using SolidTradeServer.Data.Dtos.Warrant.TradeRepublic;
using SolidTradeServer.Data.Models.Errors;
using SolidTradeServer.Services.Cache;
using WebSocketSharp;
using Timer = System.Timers.Timer;

namespace SolidTradeServer.Services.Common
{
    public class TradeRepublicApiService
    {
        private readonly ILogger _logger = Log.ForContext<TradeRepublicApiService>();
        private readonly Dictionary<int, Action<string>> _runningRequests = new();
        private readonly WebSocket _webSocket;

        private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
        private readonly Timer _timer;
        private int _latestId;
        
        public TradeRepublicApiService(IConfiguration configuration)
        // public TradeRepublicApiService(IConfiguration configuration, ICacheService cache)
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
                _logger.Fatal("Trade republic socket connection closed unexpectedly.");
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

        // We should cache this result
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
            if (e.Data == "connected" || e.Data.StartsWith("echo"))
                return;

            var id = ParseId(e.Data);

            if (id == -1)
                return;

            if (!_runningRequests.ContainsKey(id))
                return;

            _webSocket.Send($"unsub {id}");
            
            var message = GetMessageResponse(e.Data);

            _runningRequests[id].Invoke(message);
            _runningRequests.Remove(id);
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