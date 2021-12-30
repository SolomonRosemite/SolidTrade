using System.Collections.Generic;
using System.Threading.Tasks;
using OneOf;
using SolidTradeServer.Data.Dtos.User.Request;
using SolidTradeServer.Data.Dtos.User.Response;

namespace SolidTradeServer.Services
{
    public class UserService
    {
        public async Task<OneOf<CreateUserResponseDto, ErrorResponseModel>> CreateUser(CreateUserRequestDto data)
        {
            // Todo: Implement Create User handler.
            return await Task.FromResult(new CreateUserResponseDto());
        }

        public async Task<OneOf<GetUserResponseDto, ErrorResponseModel>> GetUserById(int id)
        {
            // Todo: Implement Get User handler.
            return await Task.FromResult(new GetUserResponseDto());
        }

        public async Task<OneOf<IEnumerable<GetUserResponseDto>, ErrorResponseModel>> GetUserByUsername(string username)
        {
            // Todo: Implement Get User handler.
            return await Task.FromResult(new List<GetUserResponseDto>
            {
                
            });
        }
    }
}