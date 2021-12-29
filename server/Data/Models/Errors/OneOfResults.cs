using SolidTradeServer.Data.Models.Errors.Base;

namespace SolidTradeServer.Data.Models.Errors
{
    public class InvalidJsonFormat : BaseErrorModel { }
    public class ClientDisconnected : BaseErrorModel { }
    public class BadRequest : BaseErrorModel { }
    public class NotAuthenticated : BaseErrorModel { }
}