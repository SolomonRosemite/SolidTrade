using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SolidTradeServer.Common;
using SolidTradeServer.Data.Dtos.Warrant.Request;
using SolidTradeServer.Services;
using SolidTradeServer.Services.Common;

namespace SolidTradeServer.Controllers
{
    [ApiController]
    [Route("/warrants")]
    public class WarrantController : Controller
    {
        private readonly WarrantService _warrantService;

        public WarrantController(WarrantService warrantService)
        {
            _warrantService = warrantService;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
            => CommonService.MatchResult(
                await _warrantService.GetWarrant(id, Request.Headers[Constants.UidHeader]));

        [HttpPost]
        public async Task<IActionResult> BuyWarrant([FromBody] BuyOrSellWarrantRequestDto dto)
            => CommonService.MatchResult(
                await _warrantService.BuyWarrant(dto, Request.Headers[Constants.UidHeader]));

        [HttpDelete]
        public async Task<IActionResult> SellWarrant([FromBody] BuyOrSellWarrantRequestDto dto)
            => CommonService.MatchResult(
                await _warrantService.SellWarrant(dto, Request.Headers[Constants.UidHeader]));
    }
}