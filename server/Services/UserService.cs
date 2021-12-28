using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using SolidTradeServer.Data.Dtos.Messaging;
using SolidTradeServer.Data.Dtos.User.Request;
using SolidTradeServer.Data.Dtos.User.Response;
using SolidTradeServer.Data.Models.Errors;
using SolidTradeServer.Services.Common;

namespace SolidTradeServer.Services
{
    public class UserService
    {
        public async Task<ResponseDto> CreateUser(MessageDto message)
        {
            Func<CreateUserRequestDto, Task<CreateUserResponseDto>> func = CreateUser;
            return await CommonService.HandleRequestMessage(message, func);
        }

        private async Task<CreateUserResponseDto> CreateUser(CreateUserRequestDto data)
        {
            // Todo: Implement Create User handler.
            return await Task.FromResult(new CreateUserResponseDto());
        }

        public async Task<ResponseDto> GetUser(MessageDto message)
        {
            Func<GetUserRequestDto, Task<GetUserResponseDto>> func = GetUser;
            return await CommonService.HandleRequestMessage(message, func);
        }

        private async Task<GetUserResponseDto> GetUser(GetUserRequestDto data)
        {
            // Todo: Implement Get User handler.
            return await Task.FromResult(new GetUserResponseDto());
        }
    }
}