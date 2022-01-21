using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SolidTradeServer.Services;
using static SolidTradeServer.Common.Shared;

namespace SolidTradeServer.Controllers
{
    [ApiController]
    [Route("/historicalpositions")]
    public class HistoricalPositionsController : Controller
    {
        private readonly HistoricalPositionsService _historicalPositionsService;

        public HistoricalPositionsController(HistoricalPositionsService historicalPositionsService)
        {
            _historicalPositionsService = historicalPositionsService;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
            => MatchResult(
                await _historicalPositionsService.GetHistoricalPositions(id, Request.Headers[UidHeader]));
    }
}