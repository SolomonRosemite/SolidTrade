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

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto dto)
            => CommonService.MatchResult(await _userService.CreateUser(dto));
        
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
            => CommonService.MatchResult(await _userService.GetUserById(id));
        
        [HttpGet("{username}")]
        public async Task<IActionResult> GetByUsername(string username)
            => CommonService.MatchResult(await _userService.GetUserByUsername(username));
        
        [HttpPatch]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto dto)
            => CommonService.MatchResult(await _userService.UpdateUser(dto));

        [HttpDelete]
        public async Task<IActionResult> DeleteUser()
            => CommonService.MatchResult(await _userService.DeleteUser(new DeleteUserRequestDto
            {
                Token = Request.Headers["Authorization"],
            }));
    }
}