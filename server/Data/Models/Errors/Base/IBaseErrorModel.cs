using System;

namespace SolidTradeServer.Data.Models.Errors.Base
{
    public interface IBaseErrorModel
    {
        public string ClassName { get; }
        public string Title { get; }
        public string Message { get; }
        public string UserFriendlyMessage { get; }
        public Exception Exception { get; }
    }
}