using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SolidTradeServer.Services.Common;

namespace SolidTradeServer.Controllers.WebSockets
{
    public class WebSocketController : ControllerBase
    {
        private readonly SocketRequestHandler _handler;
        
        public WebSocketController(SocketRequestHandler handler)
        {
            _handler = handler;
        }
        
        [HttpGet("/hub")]
        public async Task ConnectSocket()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await Task.Run(() => _handler.HandleSocketMessage(webSocket));
            }
            else
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        }
    }
}