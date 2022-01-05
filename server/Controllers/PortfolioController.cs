using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SolidTradeServer.Common;
using SolidTradeServer.Data.Dtos.Portfolio.Request;
using SolidTradeServer.Data.Dtos.User.Request;
using SolidTradeServer.Services;
using SolidTradeServer.Services.Common;

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
            => CommonService.MatchResult(await _portfolioService.GetPortfolio(dto, Request.Headers[Constants.UidHeader]));
    }
}