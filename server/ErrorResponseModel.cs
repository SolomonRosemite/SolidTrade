using System.Net;
using SolidTradeServer.Data.Models.Errors.Base;

namespace SolidTradeServer
{
    public class ErrorResponseModel
    {
        public ErrorResponseModel(IBaseErrorModel error, HttpStatusCode code)
        {
            Error = error;
            Code = code;
        }

        public HttpStatusCode Code { get; }
        public IBaseErrorModel Error { get; }
    }
}