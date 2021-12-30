using System;
using System.Threading.Tasks;
using SolidTradeServer.Data.Dtos.Messaging;
using SolidTradeServer.Data.Dtos.Warrant.Request;
using SolidTradeServer.Data.Dtos.Warrant.Response;
using SolidTradeServer.Services.Common;

namespace SolidTradeServer.Services
{
    public class WarrantService
    {
        public async Task<ResponseDto> HandleGetWarrant(MessageDto message)
        {
            Func<GetWarrantRequestDto, Task<GetWarrantResponseDto>> func = GetWarrant;
            // return await CommonService.HandleRequestMessage(message, func);
            return null;
        }

        private async Task<GetWarrantResponseDto> GetWarrant(GetWarrantRequestDto data)
        {
            // Todo: Implement Get Warrant handler.
            return await Task.FromResult(new GetWarrantResponseDto());
        }
    }
}