using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using Serilog;
using SolidTradeServer.Data.Dtos.Warrant.TradeRepublic;
using SolidTradeServer.Data.Models.Common.Position;
using SolidTradeServer.Data.Models.Enums;
using SolidTradeServer.Data.Models.Errors;
using SolidTradeServer.Data.Models.Errors.Common;
using Constants = SolidTradeServer.Common.Constants;

namespace SolidTradeServer.Services.Common
{
    public static class CommonService
    {
        public static FirestoreDb Firestore { get; set; }
        private static readonly ILogger _logger = Log.Logger;

        public static IActionResult MatchResult<T>(OneOf<T, ErrorResponse> value)
        {
            return value.Match(
                response => new ObjectResult(response),
                err =>
                {
                    _logger.Error(Constants.LogMessageTemplate, err.Error);
                    
                    return new ObjectResult(new UnexpectedError
                    {
                        Title = err.Error.Title,
                        UserFriendlyMessage = err.Error.UserFriendlyMessage,
                        Message = err.Error.Message,
                    }) {StatusCode = (int) err.Code};
                });
        }

        public static IPosition CalculateNewPosition(IPosition p1, IPosition p2)
        {
            Position position = new Position
            {
                NumberOfShares = p1.NumberOfShares + p2.NumberOfShares,
            };

            position.BuyInPrice =
                (p1.BuyInPrice * p1.NumberOfShares +
                 p2.BuyInPrice * p2.NumberOfShares) / position.NumberOfShares;

            return position;
        }
        
        public static string CleanIsin(string isin)
        {
            var i = isin.IndexOf('.');
            return i == -1 ? isin.Trim().ToUpper() : isin[..i].Trim().ToUpper();
        }

        // public static Func<TradeRepublicProductPriceResponseDto, decimal, bool> GetOngoingProductHandler(EnterOrExitPositionType type)
        public static bool GetOngoingProductHandler(EnterOrExitPositionType type, TradeRepublicProductPriceResponseDto value, decimal price)
        {
            return type switch
            {
                // Current has to be below
                EnterOrExitPositionType.BuyLimitOrder => price >= value.Ask.Price,
                // Current has to be above
                EnterOrExitPositionType.BuyStopOrder => value.Ask.Price >= price,
                // Current has to be above (take profit)
                EnterOrExitPositionType.SellLimitOrder => price <= value.Bid.Price,
                // Current has to be below (stop loss)
                EnterOrExitPositionType.SellStopOrder => value.Bid.Price <= price,
            };
        }
    }
}
