using Newtonsoft.Json;
using SolidTradeServer.Data.Models.Errors.Base;

namespace SolidTradeServer.Data.Models.Errors
{
    public class NotAuthenticated : BaseErrorModel { }
    public class NotAuthorized : BaseErrorModel { }
    public class UnexpectedError : BaseErrorModel { }
    public class NotFound : BaseErrorModel { }
    
    public class UserCreateFailed : BaseErrorModel { }
    public class UserUpdateFailed : BaseErrorModel { }
    public class UserDeleteFailed : BaseErrorModel { }
        
    public class TradeFailed : BaseErrorModel { }
    public class InsufficientFounds : BaseErrorModel { }
    public class StockMarketClosed : BaseErrorModel { }
}