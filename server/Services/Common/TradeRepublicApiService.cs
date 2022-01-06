using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;
using SolidTradeServer.Common;
using SolidTradeServer.Data.Models.Errors;
using WebSocketSharp;
using Timer = System.Timers.Timer;

namespace SolidTradeServer.Services.Common
{
    public class TradeRepublicApiService
    {
        private readonly ILogger _logger = Log.ForContext<TradeRepublicApiService>();
        private readonly Dictionary<int, Action<string>> _runningRequests = new();
        private readonly WebSocket _webSocket;
        
        private readonly Timer _timer; 
        
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
                _logger.Fatal("Trade republic socket connection closed unexpectedly.");
            };
            
            _webSocket.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
            _webSocket.Connect();
        }

        public async Task<string> AddRequest(string content, CancellationToken token)
        {
            var tcs = new TaskCompletionSource<string>();
            var id = _runningRequests.Count + 1;
            
            _runningRequests.Add(id, response =>
            {
                tcs.SetResult(response);
            });

            _webSocket.Send($"sub {id} {content}");

            while (!tcs.Task.IsCompleted)
                await Task.Delay(50, token);
            
            return await tcs.Task;
        }

        private void OnTradeRepublicMessage(object? sender, MessageEventArgs e)
        {
            if (e.Data == "connected" || e.Data.StartsWith("echo"))
                return;

            var id = ParseId(e.Data);

            if (id == -1)
                return;

            var message = GetMessageResponse(e.Data);

            _runningRequests[id].Invoke(message);
            _runningRequests.Remove(id);
            
            _webSocket.Send($"unsub {id}");
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
        
        public void InitTimer()
        {
            _timer.AutoReset = true;
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }

        private void TimerOnElapsed(object sender, EventArgs e)
        {
            _webSocket.Send("echo " + DateTime.Now.Ticks);
        }
    }
}