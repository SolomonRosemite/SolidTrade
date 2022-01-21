using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using SolidTradeServer.Data.Dtos.TradeRepublic;
using SolidTradeServer.Data.Models.Common.Position;
using SolidTradeServer.Data.Models.Enums;
using SolidTradeServer.Data.Models.Errors;
using SolidTradeServer.Data.Models.Errors.Common;
using SolidTradeServer.Services.TradeRepublic;

namespace SolidTradeServer.Common
{
    public static class Shared
    {
        private static readonly ILogger _logger = Log.Logger;
        
        public const string LogMessageTemplate = "{@LogParameters}"; 
        public static string UidHeader => "_Uid";

        public static string GetTradeRepublicProductInfoRequestString(string isin)
            => "{\"type\":\"instrument\",\"id\":\"" + isin + "\"}";

        public static string GetTradeRepublicProductPriceRequestString(string isin)
            => "{\"type\":\"ticker\",\"id\":\"" + isin + "\"}";
        
        private static readonly JsonSerializerSettings _jsonSerializerOptions = new()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
        };
        
        public static OneOf<T, UnexpectedError> ConvertToObject<T>(string content, JsonSerializerSettings jsonSerializerOptions = null)
        {
            jsonSerializerOptions ??= _jsonSerializerOptions;
            
            try
            {
                var result = JsonConvert.DeserializeObject<T>(content, jsonSerializerOptions);

                var isInvalidObject = result!.GetType().GetProperties()
                    .Select(pi => pi.GetValue(result)).All(value => value is null);

                if (isInvalidObject)
                    throw new Exception("All json properties were set null when trying to deserialize json string.");

                return result;
            }
            catch (Exception e)
            {
                return new UnexpectedError
                {
                    Title = "Json parsing error",
                    Message = "Parsing Trade Republic message response failed.",
                    Exception = e,
                    AdditionalData = new { Response = content, Type = typeof(T) },
                };
            }
        }
        
                
        public static string ToIsinWithoutExchangeExtension(string isin)
        {
            var i = isin.IndexOf('.');
            return i == -1 ? isin.Trim().ToUpper() : isin[..i].Trim().ToUpper();
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
                
        public static IActionResult MatchResult<T>(OneOf<T, ErrorResponse> value)
        {
            return value.Match(
                response => new ObjectResult(response),
                err =>
                {
                    var ex = err.Error.Exception;
                    err.Error.Exception = ex is not null ? new Exception("Exception is defined in the 'exceptions' field.") : null;
                    _logger.Error(ex, LogMessageTemplate, err.Error);

                    return new ObjectResult(new UnexpectedError
                    {
                        Title = err.Error.Title,
                        UserFriendlyMessage = err.Error.UserFriendlyMessage,
                        Message = err.Error.Message,
                    }) {StatusCode = (int) err.Code};
                });
        }
        
        public static string GetOrderName(EnterOrExitPositionType type)
        {
            return type switch
            {
                EnterOrExitPositionType.BuyLimitOrder => "buy limit",
                EnterOrExitPositionType.BuyStopOrder => "buy stop",
                EnterOrExitPositionType.SellLimitOrder => "take profit",
                EnterOrExitPositionType.SellStopOrder => "stop loss",
            };
        }

        public static BuyOrSell IsBuyOrSell(EnterOrExitPositionType type)
        {
            return type switch
            {
                EnterOrExitPositionType.BuyLimitOrder => BuyOrSell.Buy,
                EnterOrExitPositionType.BuyStopOrder => BuyOrSell.Buy,
                EnterOrExitPositionType.SellLimitOrder => BuyOrSell.Sell,
                EnterOrExitPositionType.SellStopOrder => BuyOrSell.Sell,
            };
        }
        
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
        
        public static async Task<OneOf<T, ErrorResponse>> MakeTrRequestWithService<T>(TradeRepublicApi trService, string requestString)
        {
            var cts = new CancellationTokenSource();
            T trResponse;
            
            try
            {
                cts.CancelAfter(1000 * 10);
                var oneOfResult =
                    await trService.AddRequest<T>(requestString, cts.Token);

                if (oneOfResult.TryPickT1(out var error, out trResponse))
                    return new ErrorResponse(error, HttpStatusCode.InternalServerError);
            }
            catch (OperationCanceledException e)
            {
                return new ErrorResponse(new UnexpectedError
                {
                    Title = "Task timeout",
                    Message = "Fetching product using trade republic api took too long.",
                    AdditionalData = new { requestString },
                    Exception = e,
                }, HttpStatusCode.InternalServerError);
            }
            finally { cts.Dispose(); }

            return trResponse;
        }
    }
}