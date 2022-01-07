using System;

namespace SolidTradeServer.Data.Models.Errors.Base
{
    public class BaseErrorModel : IBaseErrorModel
    {
        public string Title { get; init; }
        public string Message { get; init; }
        public string UserFriendlyMessage { get; init; }
        public object AdditionalData { get; init; }
        public Exception Exception { get; set; }
    }
}