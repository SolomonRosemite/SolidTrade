using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SolidTradeServer.Common;
using SolidTradeServer.Data.Dtos.Shared.Common;
using SolidTradeServer.Services;
using static SolidTradeServer.Common.Shared;

namespace SolidTradeServer.Controllers
{
    [ApiController]
    [Route("/stocks")]
    public class StockController : Controller
    {
        private readonly StockService _stockService;

        public StockController(StockService stockService)
        {
            _stockService = stockService;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
            => MatchResult(
                await _stockService.GetStock(id, Request.Headers[Shared.UidHeader]));

        [HttpPost]
        public async Task<IActionResult> BuyStock([FromBody] BuyOrSellRequestDto dto)
            => MatchResult(
                await _stockService.BuyStock(dto, Request.Headers[Shared.UidHeader]));

        [HttpDelete]
        public async Task<IActionResult> SellStock([FromBody] BuyOrSellRequestDto dto)
            => MatchResult(
                await _stockService.SellStock(dto, Request.Headers[Shared.UidHeader]));
    }
}