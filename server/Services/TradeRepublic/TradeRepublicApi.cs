using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OneOf;
using Serilog;
using SolidTradeServer.Common;
using SolidTradeServer.Data.Dtos.TradeRepublic;
using SolidTradeServer.Data.Models.Common.Log;
using SolidTradeServer.Data.Models.Enums;
using SolidTradeServer.Data.Models.Errors;
using WebSocketSharp;
using static SolidTradeServer.Common.Shared;
using TradeRepublicProductInfoDto = SolidTradeServer.Data.Dtos.TradeRepublic.TradeRepublicProductInfoDto;

namespace SolidTradeServer.Services.TradeRepublic
{
    public abstract class TradeRepublicApi
    {
        private readonly Dictionary<int, (PositionType, int)> _runningRequestsAsync = new();
        private readonly Dictionary<int, Action<string>> _runningRequests = new();
        private readonly Dictionary<int, string> _requestBodies = new();

        private readonly ILogger _logger = Log.ForContext<TradeRepublicApi>();
        private readonly OngoingProductsService _ongoingProductsService;
        private readonly WebSocket _webSocket;
        private bool _isReconnect;
        
        private DateTime _lastMessageReceived = DateTime.Now;

        private int _latestId;
        
        protected TradeRepublicApi(IConfiguration configuration, OngoingProductsService ongoingProductsService)
        {
            _ongoingProductsService = ongoingProductsService;
            _webSocket = new WebSocket(configuration["TradeRepublic:ApiEndpoint"]);
            
            _webSocket.OnMessage += OnTradeRepublicMessage;
            _webSocket.OnOpen += (_, _) =>
            {
                _logger.Information("Connected to Trade republic api");
                _webSocket.Send(configuration["TradeRepublic:InitialConnectString"]);

                if (!_isReconnect)
                    Task.Run(async () => await RegisterAllOngoingPosition());
            };

            _webSocket.OnClose += (_, _) =>
            {
                _logger.Warning("Trade republic api connection closed unexpectedly");
                _logger.Information("Trying to reconnect");

                _isReconnect = true;
                _webSocket.Connect();
            };

            _webSocket.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
            _webSocket.Connect();
        }
        
        public void AddOngoingRequest(string isin, PositionType positionType, int entityId)
        {
            var id = GetNewId();
            _runningRequestsAsync.Add(id, (positionType, entityId));

            string content = "{\"type\":\"ticker\",\"id\":\"" + isin + "\"}";
            
            _webSocket.Send($"sub {id} {content}");
            _requestBodies.Add(id, content);
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
            _requestBodies.Add(id, content);

            while (!tcs.Task.IsCompleted)
                await Task.Delay(50, token);
            
            return await tcs.Task;
        }

        private void OnTradeRepublicMessage(object sender, MessageEventArgs e)
        {
            _logger.Information("{@TradeRepublicMessage}", new TradeRepublicMessage
            {
                Title = "Trade Republic api message",
                Content = e.Data,
            });

            if (_isReconnect && e.Data == "connected")
            {
                var runningRequestBodies = new Dictionary<int, string>(_requestBodies);
                
                foreach ((int requestId, string requestBody) in runningRequestBodies)
                {
                    if (_runningRequestsAsync.ContainsKey(requestId))
                        _webSocket.Send($"sub {requestId} {requestBody}");
                    else
                        _requestBodies.Remove(requestId);
                }

                _isReconnect = false;
            }
            
            if (e.Data == "connected" || e.Data.StartsWith("echo"))
                return;

            if (DateTime.Now.Subtract(_lastMessageReceived).TotalSeconds > 30)
            {
                _webSocket.Send($"echo {_lastMessageReceived.Ticks}");
                _lastMessageReceived = DateTime.Now;
            }

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
                var (type, productId) = _runningRequestsAsync[id];
                Task.Run(() => HandleRequestMessage(id, productId, type, message));
            }
        }

        private static string GetMessageResponse(string messageInput)
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
                _logger.Error(LogMessageTemplate, new UnexpectedError
                {
                  Title  = "Failed to parse id",
                  Message = "Failed to parse id from trade republic message.",
                  Exception = e,
                  AdditionalData = new { message }
                });
                return -1;
            }
        }

        private int GetNewId()
            => ++_latestId;

        private async Task RegisterAllOngoingPosition()
        {
            try
            {
                var (ongoingWarrantPositions, ongoingKnockoutPositions) = _ongoingProductsService.GetAllOngoingPositions();

                var oneOfExchangeResults = new ConcurrentBag<Task<OneOf<TradeRepublicProductInfoDto, UnexpectedError>>>();
                
                string requestStr;
                foreach (var warrantPosition in ongoingWarrantPositions)
                {
                    requestStr = "{\"type\":\"instrument\", \"id\":\"" + warrantPosition.Isin + "\"}";
                    oneOfExchangeResults.Add(AddRequest<TradeRepublicProductInfoDto>(requestStr, CancellationToken.None));
                }
                
                foreach (var knockoutPosition in ongoingKnockoutPositions)
                {
                    requestStr = "{\"type\":\"instrument\", \"id\":\"" + knockoutPosition.Isin + "\"}";
                    oneOfExchangeResults.Add(AddRequest<TradeRepublicProductInfoDto>(requestStr, CancellationToken.None));
                }

                var results = await Task.WhenAll(oneOfExchangeResults);

                if (results.Any(r => r.IsT1))
                {
                    _logger.Fatal(LogMessageTemplate, results.First(r => r.IsT1).AsT1);
                    Thread.Sleep(1000);
                    Program.ExitApplication();
                }

                var ids = results.Select(o => o.AsT0);

                for (int i = 0; i < ongoingWarrantPositions.Count; i++)
                {
                    var productInfo = ids.First(p => p.Isin == ongoingWarrantPositions[i].Isin);
                    
                    if (productInfo.Active.HasValue && !productInfo.Active.Value)
                    {
                        _logger.Information(
                            "Ongoing warrant with id {@Id} and isin {@Isin} is not active anymore and wont be added to ongoing requests",
                            ongoingWarrantPositions[i].Id, ongoingWarrantPositions[i].Isin);
                        continue;
                    }
                    
                    AddOngoingRequest(ongoingWarrantPositions[i].Isin + "." + productInfo.ExchangeIds.First(), PositionType.Warrant, ongoingWarrantPositions[i].Id);
                }

                for (int i = 0; i < ongoingKnockoutPositions.Count; i++)
                {
                    var productInfo = ids.First(p => p.Isin == ongoingKnockoutPositions[i].Isin);
                    
                    if (productInfo.Active.HasValue && !productInfo.Active.Value)
                    {
                        _logger.Information(
                            "Ongoing knockout with id {@Id} and isin {@Isin} is not active anymore and wont be added to ongoing requests",
                            ongoingKnockoutPositions[i].Id, ongoingKnockoutPositions[i].Isin);
                        continue;
                    }

                    AddOngoingRequest(ongoingKnockoutPositions[i].Isin + "." + productInfo.ExchangeIds.First(), PositionType.Knockout, ongoingKnockoutPositions[i].Id);
                }
            }
            catch (Exception e)
            {
                _logger.Fatal(LogMessageTemplate, new UnexpectedError
                {
                    Title = "Failed register ongoing positions",
                    Message = "Unexpected fatal error while trying to register ongoing positions",
                    Exception = e,
                });
                Thread.Sleep(1000);
                Program.ExitApplication();
            }
        }
        
        private void HandleRequestMessage(int id, int productId, PositionType positionType, string message)
        {
            if (ConvertToObject<TradeRepublicProductPriceResponseDto>(message).TryPickT0(out var value, out var err))
            {
                try
                {
                    var result = GetOngoingTradeResponse(this, value, positionType, productId);

                    switch (result)
                    {
                        case OngoingTradeResponse.WaitingForFill:
                            break;
                        case OngoingTradeResponse.Complete:
                        case OngoingTradeResponse.PositionsAlreadyClosed:
                        case OngoingTradeResponse.Failed:
                            _webSocket.Send($"unsub {id}");
                            _runningRequestsAsync.Remove(id);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(LogMessageTemplate, new UnexpectedError
                    {
                        Title = "Processing ongoing trade failed",
                        Exception = e,
                        AdditionalData = new {Dto = value}
                    });
                    _webSocket.Send($"unsub {id}");
                    _runningRequestsAsync.Remove(id);
                }
            }
            else
            {
                _logger.Error(LogMessageTemplate, err);

                _webSocket.Send($"unsub {id}");
                _runningRequestsAsync.Remove(id);
            }
        }
        
        private OngoingTradeResponse GetOngoingTradeResponse(TradeRepublicApi trService, TradeRepublicProductPriceResponseDto value, PositionType positionType, int productId)
        {
            return positionType switch
            {
                PositionType.Warrant => _ongoingProductsService.HandleOngoingWarrantTradeMessage(trService, value, positionType, productId),
                PositionType.Knockout => _ongoingProductsService.HandleOngoingKnockoutTradeMessage(trService, value, positionType, productId),
                PositionType.Stock => OngoingTradeResponse.Failed,
            };
        }
     }
}