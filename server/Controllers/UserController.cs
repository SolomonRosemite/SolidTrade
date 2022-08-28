using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SolidTradeServer.Common;
using SolidTradeServer.Data.Dtos.User.Request;
using SolidTradeServer.Services;
using static SolidTradeServer.Common.Shared;

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

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromForm] CreateUserRequestDto dto)
            => MatchResult(await _userService.CreateUser(dto, Request.Headers[Shared.UidHeader]));
        
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
            => MatchResult(await _userService.GetUserById(id, Request.Headers[Shared.UidHeader]));
        
        [HttpGet("{username}")]
        public async Task<IActionResult> GetByUsername(string username)
            => MatchResult(await _userService.SearchUserByUsername(username, Request.Headers[Shared.UidHeader]));
        
        [HttpGet]
        public async Task<IActionResult> GetByUid([FromQuery] GetUserByUidRequestDto dto)
            => MatchResult(await _userService.GetUserByUid(dto.Uid, Request.Headers[Shared.UidHeader]));

        [HttpPatch]
        public async Task<IActionResult> UpdateUser([FromForm] UpdateUserDto dto)
            => MatchResult(await _userService.UpdateUser(dto, Request.Headers[Shared.UidHeader]));

        [HttpDelete]
        public async Task<IActionResult> DeleteUser()
            => MatchResult(await _userService.DeleteUser(Request.Headers[Shared.UidHeader]));
    }
}