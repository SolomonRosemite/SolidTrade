using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SolidTradeServer.Common;
using SolidTradeServer.Data.Dtos.Shared.OngoingPosition.Request;
using SolidTradeServer.Services;
using SolidTradeServer.Services.Common;

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
            => CommonService.MatchResult(
                await _ongoingWarrantService.GetOngoingWarrant(id, Request.Headers[Constants.UidHeader]));

        [HttpPost]
        public async Task<IActionResult> OpenOngoingWarrant([FromBody] OngoingPositionRequestDto dto)
            => CommonService.MatchResult(
                await _ongoingWarrantService.OpenOngoingWarrant(dto, Request.Headers[Constants.UidHeader]));

        [HttpDelete]
        public async Task<IActionResult> CloseOngoingWarrant([FromBody] CloseOngoingPositionRequestDto dto)
            => CommonService.MatchResult(
                await _ongoingWarrantService.CloseOngoingWarrant(dto, Request.Headers[Constants.UidHeader]));
    }
}