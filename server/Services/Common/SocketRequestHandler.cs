using System;
using OneOf;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using SolidTradeServer.Data.Dtos.Messaging;
using SolidTradeServer.Data.Models.Enums;
using SolidTradeServer.Data.Models.Errors;
using WebSocketSharpClient = WebSocketSharp.WebSocket;

namespace SolidTradeServer.Services.Common
{
    public class SocketRequestHandler
    {
        private readonly WebSocketSharpClient _tradeRepublicApiSocket;
        private readonly ILogger _logger;
        private WebSocket _clientSocket;
        
        private readonly AuthenticationService _authService;
        private readonly WarrantService _warrantService;
        private readonly UserService _userService;

        public SocketRequestHandler(ILogger logger, AuthenticationService authService, UserService userService,
            WarrantService warrantService)
        {
            _tradeRepublicApiSocket = new WebSocketSharpClient("wss://api.traderepublic.com/");

            _warrantService = warrantService;
            _authService = authService;
            _userService = userService;
            _logger = logger;
        }

        public async Task HandleSocketMessage(WebSocket webSocket)
        {
            _clientSocket = webSocket;
            
            WebSocketReceiveResult prevSocketReceiveResult = null;
            var firstRun = true;
            do
            {
                var buffer = new byte[1024 * 4];
                var successful = (await ReceiveMessageAsync(webSocket, buffer))
                    .TryPickT0(out var result, out var otherErr);

                if (!successful)
                {
                    var disconnected = await otherErr.Match(async invalidJsonError =>
                    {
                        await SendMessage(webSocket, ResponseDto.Failed(-1, MessageType.MessageTypeUnspecified, invalidJsonError));
                        return false;
                    }, _ => Task.FromResult(true));

                    if (disconnected)
                        break;
                    
                    continue;
                }
                
                var (webSocketReceiveResult, messageDto) = result;
                prevSocketReceiveResult = webSocketReceiveResult;
                firstRun = false;

                var isAuthenticated = await _authService.AuthenticateUser(messageDto.Metadata);

                if (!isAuthenticated)
                {
                    await SendMessage(webSocket, ResponseDto.Failed(messageDto, new NotAuthenticated
                    {
                        Title = "Not Authenticated",
                        Message = "The token provided is not valid or may be expired.",
                    }));
                    continue;
                }
                
                var response = await HandleMessage(messageDto);

                await SendMessage(webSocket, response);
            } while (firstRun || !prevSocketReceiveResult.CloseStatus.HasValue);

            if (prevSocketReceiveResult?.CloseStatus is not null)
                await webSocket.CloseAsync(prevSocketReceiveResult.CloseStatus!.Value,
                    prevSocketReceiveResult.CloseStatusDescription, CancellationToken.None);

            webSocket.Dispose();
        }
        
        private async Task<ResponseDto> HandleMessage(MessageDto message)
        {
            return message.MessageType switch
            {
                // MessageType.GetUser => await _userService.GetUser(message),
                // MessageType.CreateUser => await _userService.CreateUser(message),
                MessageType.GetWarrant => await _warrantService.HandleGetWarrant(message),
                MessageType.MessageTypeUnspecified => ResponseDto.Failed(message, new BadRequest
                {
                    Title = "Message type missing",
                    Message = "Message type was not specified.",
                }),
                _ => throw new Exception(),
            };
        }

        private static async Task<OneOf<(WebSocketReceiveResult, MessageDto), InvalidJsonFormat, ClientDisconnected>> ReceiveMessageAsync(WebSocket webSocket,
            byte[] buffer, WebSocketReceiveResult webSocketReceiveResult = null)
        {
            var socketResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (socketResult.CloseStatus.HasValue)
                return new ClientDisconnected();
            
            var result = MessageDto.ToMessage(buffer);

            if (result.TryPickT1(out var invalidJsonFormat, out var messageDto))
                return invalidJsonFormat;

            return (webSocketReceiveResult ?? socketResult, messageDto);
        }

        private static Task SendMessage(WebSocket webSocket, ResponseDto response)
        {
            string content = response.ToJsonString();
            
            return webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(content), 0, content.Length),
                WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}