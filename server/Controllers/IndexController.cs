using SolidTradeServer.Data.Dtos.HealthCheck;
using Microsoft.AspNetCore.Mvc;

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