using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SolidTradeServer.Common;
using SolidTradeServer.Data.Dtos.Shared.OngoingPosition.Request;
using SolidTradeServer.Services;
using static SolidTradeServer.Common.Shared;

namespace SolidTradeServer.Controllers
{
    [ApiController]
    [Route("/knockouts/ongoing")]
    public class OngoingKnockoutController : Controller
    {
        private readonly OngoingKnockoutService _ongoingKnockoutService;

        public OngoingKnockoutController(OngoingKnockoutService ongoingKnockoutService)
        {
            _ongoingKnockoutService = ongoingKnockoutService;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
            => MatchResult(
                await _ongoingKnockoutService.GetOngoingKnockout(id, Request.Headers[Shared.UidHeader]));

        [HttpPost]
        public async Task<IActionResult> OpenOngoingWarrant([FromBody] OngoingPositionRequestDto dto)
            => MatchResult(
                await _ongoingKnockoutService.OpenOngoingKnockout(dto, Request.Headers[Shared.UidHeader]));

        [HttpDelete]
        public async Task<IActionResult> CloseOngoingWarrant([FromBody] CloseOngoingPositionRequestDto dto)
            => MatchResult(
                await _ongoingKnockoutService.CloseOngoingKnockout(dto, Request.Headers[Shared.UidHeader]));
    }
}