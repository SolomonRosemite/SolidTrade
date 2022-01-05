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
            => CommonService.MatchResult(
                await _historicalPositionsService.GetHistoricalPositions(id, Request.Headers[Constants.UidHeader]));
    }
}