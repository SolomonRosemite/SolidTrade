using System;
using System.Threading.Tasks;
using SolidTradeServer.Data.Dtos.Messaging;

namespace SolidTradeServer.Services.Common
{
    public static class CommonService
    {
        public static async Task<ResponseDto> HandleRequestMessage<TResult, TDto>(MessageDto message, Func<TDto, Task<TResult>> func)
        {
            return await message.CastTo<TDto>()
                .Match(
                    async requestDto => ResponseDto.Success(message, await func(requestDto)),
                    async err => 
                        await Task.Run(() => ResponseDto.Failed(message, err)));
        }
    }
}