using SolidTradeServer.Data.Models.Errors.Common;
using SolidTradeServer.Data.Models.Interfaces;

namespace SolidTradeServer.Data.Dtos.User.Response
{
    public class CreateUserResponseDto : IResponseDtoModel
    {
        public ResponseModelError Error { get; init; }
    }
}