using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SolidTradeServer.Common;
using SolidTradeServer.Data.Dtos.Shared.OngoingPosition.Request;
using SolidTradeServer.Services;
using static SolidTradeServer.Common.Shared;

namespace SolidTradeServer.Controllers
{
    [ApiController]
    [Route("/warrants/ongoing")]
    public class OngoingWarrantController : Controller
    {
        private readonly OngoingWarrantService _ongoingWarrantService;

        public OngoingWarrantController(OngoingWarrantService ongoingWarrantService)
        {
            _ongoingWarrantService = ongoingWarrantService;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
            => MatchResult(
                await _ongoingWarrantService.GetOngoingWarrant(id, Request.Headers[Shared.UidHeader]));

        [HttpPost]
        public async Task<IActionResult> OpenOngoingWarrant([FromBody] OngoingPositionRequestDto dto)
            => MatchResult(
                await _ongoingWarrantService.OpenOngoingWarrant(dto, Request.Headers[Shared.UidHeader]));

        [HttpDelete]
        public async Task<IActionResult> CloseOngoingWarrant([FromBody] CloseOngoingPositionRequestDto dto)
            => MatchResult(
                await _ongoingWarrantService.CloseOngoingWarrant(dto, Request.Headers[Shared.UidHeader]));
    }
}