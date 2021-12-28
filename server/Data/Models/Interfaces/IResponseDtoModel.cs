using SolidTradeServer.Data.Models.Errors.Common;

namespace SolidTradeServer.Data.Models.Interfaces
{
    public interface IResponseDtoModel
    {
        public ResponseModelError Error { get; init; }
    }
}