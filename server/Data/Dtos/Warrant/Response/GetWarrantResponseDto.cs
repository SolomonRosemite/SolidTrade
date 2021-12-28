using SolidTradeServer.Data.Models.Errors.Common;
using SolidTradeServer.Data.Models.Interfaces;

namespace SolidTradeServer.Data.Dtos.Warrant.Response
{
    public class GetWarrantResponseDto : IResponseDtoModel
    {
        public ResponseModelError Error { get; init; }
    }
}