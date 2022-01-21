using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SolidTradeServer.Common;
using SolidTradeServer.Data.Dtos.Shared.Common;
using SolidTradeServer.Services;
using static SolidTradeServer.Common.Shared;

namespace SolidTradeServer.Controllers
{
    [ApiController]
    [Route("/knockouts")]
    public class KnockoutController : Controller
    {
        private readonly KnockoutService _knockoutService;

        public KnockoutController(KnockoutService knockoutService)
        {
            _knockoutService = knockoutService;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
            => MatchResult(
                await _knockoutService.GetKnockout(id, Request.Headers[Shared.UidHeader]));

        [HttpPost]
        public async Task<IActionResult> BuyKnockout([FromBody] BuyOrSellRequestDto dto)
            => MatchResult(
                await _knockoutService.BuyKnockout(dto, Request.Headers[Shared.UidHeader]));

        [HttpDelete]
        public async Task<IActionResult> SellKnockout([FromBody] BuyOrSellRequestDto dto)
            => MatchResult(
                await _knockoutService.SellKnockout(dto, Request.Headers[Shared.UidHeader]));
    }
}