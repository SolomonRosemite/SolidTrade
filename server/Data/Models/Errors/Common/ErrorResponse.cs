using System.Net;
using SolidTradeServer.Data.Models.Errors.Base;

namespace SolidTradeServer.Data.Models.Errors.Common
{
    public class ErrorResponse
    {
        public ErrorResponse(IBaseErrorModel error, HttpStatusCode code)
        {
            Error = error;
            Code = code;
        }

        public HttpStatusCode Code { get; }
        public IBaseErrorModel Error { get; }
    }
}