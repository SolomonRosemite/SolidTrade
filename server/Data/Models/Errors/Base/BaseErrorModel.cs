using SolidTradeServer.Data.Models.Errors.Common;

namespace SolidTradeServer.Data.Models.Errors.Base
{
    public abstract class BaseErrorModel : IBaseErrorModel
    {
        public string ClassName => this.GetType().Name;
        public string Title { get; init; }
        public string Message { get; init; }
        public string UserFriendlyMessage { get; init; }
        public System.Exception Exception { get; init; }

        public System.Exception AsException()
        {
            return new ExceptionWrapper(this);
        }
    }
}