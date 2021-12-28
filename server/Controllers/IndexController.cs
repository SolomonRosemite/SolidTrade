using System.Net.WebSockets;
using System.Threading.Tasks;
using SolidTradeServer.Data.Dtos.HealthCheck;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace SolidTradeServer.Controllers
{
    [ApiController]
    [Route("/")]
    public class IndexController : Controller
    {
        [HttpGet]
        public IActionResult GetCheck()
        {
            return Ok(new GetHealthCheckDto(Request.Query, Request.Headers));
        }
        
        [HttpGet("HealthCheck")]
        public IActionResult GetHealthCheck()
        {
            return GetCheck();
        }
    }
}