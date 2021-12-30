using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SolidTradeServer.Data.Dtos.User.Request;
using SolidTradeServer.Services;
using SolidTradeServer.Services.Common;

namespace SolidTradeServer.Controllers
{
    [ApiController]
    [Route("/users")]
    public class UserController : Controller
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
            => CommonService.MatchResult(await _userService.GetUserById(id));
        
        [HttpGet("{username}")]
        public async Task<IActionResult> GetByUsername(string username)
            => CommonService.MatchResult(await _userService.GetUserByUsername(username));

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto dto)
            => CommonService.MatchResult(await _userService.CreateUser(dto));
    }
}