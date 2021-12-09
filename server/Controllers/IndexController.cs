using System;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using SolidTradeServer.Data.Dtos.HealthCheck;
using SolidTradeServer.Services;

namespace SolidTradeServer.Controllers
{
    [ApiController]
    [Route("/")]
    public class IndexController : Controller
    {
        private readonly ILogger _logger;
        private readonly IndexService _service;

        public IndexController(ILogger logger, IndexService service)
        {
            _logger = logger;
            _service = service;
        }

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
        
        [HttpGet("TemporaryLogger")]
        public IActionResult GetTemporaryLogger()
        {
            try
            {
                var weatherInfos = _service.FetchWeatherInfos();
                
                return Ok(weatherInfos);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Something didn't go as planned");
                return new StatusCodeResult(500);
            }
        }
    }
}