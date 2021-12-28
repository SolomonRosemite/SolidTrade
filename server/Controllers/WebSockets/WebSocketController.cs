using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using Serilog;
using SolidTradeServer.Data.Dtos.Messaging;
using SolidTradeServer.Data.Models.Enums;
using SolidTradeServer.Data.Models.Errors;
using SolidTradeServer.Services;

namespace SolidTradeServer.Controllers.WebSockets
{
    public class WebSocketController : ControllerBase
    {
        private readonly AuthenticationService _authService;
        private readonly WarrantService _warrantService;
        private readonly UserService _userService;
        private readonly ILogger _logger;

        public WebSocketController(ILogger logger, AuthenticationService authService, UserService userService, WarrantService warrantService)
        {
            _logger = logger;
            _authService = authService;
            _userService = userService;
            _warrantService = warrantService;
        }

        [HttpGet("/hub")]
        public async Task ConnectSocket()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using WebSocket webSocket = await 
                    HttpContext.WebSockets.AcceptWebSocketAsync();
                await HandleWebSocket(HttpContext, webSocket);
                
                return;
            }

            HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        }

        private async Task HandleWebSocket(HttpContext _, WebSocket webSocket)
        {
            WebSocketReceiveResult prevSocketReceiveResult = null;
            do
            {
                var buffer = new byte[1024 * 4];
                var failed = (await ReceiveMessageAsync(webSocket, buffer))
                    .TryPickT1(out var err, out var result);

                if (failed)
                {
                    await SendMessage(webSocket, new ResponseDto(-1, MessageType.MessageTypeUnspecified, err, false));
                    continue;
                }
                
                var (webSocketReceiveResult, messageDto) = result;
                prevSocketReceiveResult = webSocketReceiveResult;

                var isAuthenticated = await _authService.AuthenticateUser(messageDto.Metadata);

                if (!isAuthenticated)
                {
                    // Todo: Implement handle if user authentication failed.
                    return;
                }
                
                var response = await HandleMessage(messageDto);

                await SendMessage(webSocket, response);
            } while (prevSocketReceiveResult?.CloseStatus == null);

            await webSocket.CloseAsync(prevSocketReceiveResult.CloseStatus!.Value, prevSocketReceiveResult.CloseStatusDescription, CancellationToken.None);
        }

        private async Task<ResponseDto> HandleMessage(MessageDto message)
        {
            return message.MessageType switch
            {
                MessageType.GetUser => await _userService.GetUser(message),
                MessageType.CreateUser => await _userService.CreateUser(message),
                MessageType.GetWarrants => await _warrantService.HandleGetWarrant(message),
                MessageType.MessageTypeUnspecified => new ResponseDto(message, new
                {
                    Message = "Client send message with message type 'Undefined' or message type was not specified."
                }, true),
            };
        }

        private async Task<OneOf<(WebSocketReceiveResult, MessageDto), IEnumerable<InvalidJsonFormat>>> ReceiveMessageAsync(WebSocket webSocket,
            byte[] buffer, WebSocketReceiveResult webSocketReceiveResult = null)
        {
            var socketResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (socketResult.CloseStatus.HasValue)
            {
                // Todo: Implement exit here.
            }
            
            var result = MessageDto.ToMessage(buffer);

            if (result.TryPickT1(out var invalidJsonFormat, out var messageDto))
            {
                return invalidJsonFormat.ToList();
            }

            return (webSocketReceiveResult ?? socketResult, messageDto);
        }

        private Task SendMessage(WebSocket webSocket, ResponseDto response)
        {
            string content = response.ToJsonString();
            
            return webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(content), 0, content.Length),
                WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}