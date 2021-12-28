using System;
using SolidTradeServer.Data.Models.Errors.Base;

namespace SolidTradeServer.Data.Models.Errors.Common
{
    public class ExceptionWrapper : Exception, IBaseErrorModel
    {
        public ExceptionWrapper(IBaseErrorModel model) : base(model.Message)
        {
            ClassName = model.ClassName;
            Title = model.Title;
            UserFriendlyMessage = model.UserFriendlyMessage;
            Exception = model.Exception;
        }

        public string ClassName { get; }
        public string Title { get; }
        public string UserFriendlyMessage { get; }
        public Exception Exception { get; }
    }
}