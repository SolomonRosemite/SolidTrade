using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SolidTradeServer.Common;
using SolidTradeServer.Data.Dtos.Portfolio.Request;
using SolidTradeServer.Services;
using static SolidTradeServer.Common.Shared;

namespace SolidTradeServer.Controllers
{
    [ApiController]
    [Route("/portfolios")]
    public class PortfolioController : Controller
    {
        private readonly PortfolioService _portfolioService;

        public PortfolioController(PortfolioService portfolioService)
        {
            _portfolioService = portfolioService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] GetPortfolioRequestDto dto)
            => MatchResult(await _portfolioService.GetPortfolio(dto, Request.Headers[Shared.UidHeader]));
    }
}
