using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SolidTradeServer.Common;
using SolidTradeServer.Data.Dtos.Shared.Common;
using SolidTradeServer.Services;
using SolidTradeServer.Services.Common;

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
            => CommonService.MatchResult(
                await _stockService.GetStock(id, Request.Headers[Constants.UidHeader]));

        [HttpPost]
        public async Task<IActionResult> BuyStock([FromBody] BuyOrSellRequestDto dto)
            => CommonService.MatchResult(
                await _stockService.BuyStock(dto, Request.Headers[Constants.UidHeader]));

        [HttpDelete]
        public async Task<IActionResult> SellStock([FromBody] BuyOrSellRequestDto dto)
            => CommonService.MatchResult(
                await _stockService.SellStock(dto, Request.Headers[Constants.UidHeader]));
    }
}