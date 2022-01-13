using System;
using System.Collections.Generic;
using System.Linq;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OneOf;
using Serilog;
using SolidTradeServer.Data.Common;
using SolidTradeServer.Data.Dtos.Warrant.TradeRepublic;
using SolidTradeServer.Data.Entities;
using SolidTradeServer.Data.Models.Common.Position;
using SolidTradeServer.Data.Models.Enums;
using SolidTradeServer.Data.Models.Errors;
using SolidTradeServer.Data.Models.Errors.Common;
using SolidTradeServer.Services.Cache;
using Constants = SolidTradeServer.Common.Constants;

namespace SolidTradeServer.Services.Common
{
    public class CommonService
    {
        public static FirestoreDb Firestore { get; set; }

        private static readonly ILogger _logger = Log.Logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ICacheService _cache;

        public CommonService(ICacheService cache, IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _cache = cache;
        }
        
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

        public (List<OngoingWarrantPosition>, List<OngoingKnockoutPosition>) GetAllOngoingPositions()
        {
            using var db = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<DbSolidTrade>();
            return (db.OngoingWarrantPositions.ToList(), db.OngoingKnockoutPositions.ToList());
        }

        // Todo: Make this methods usable for knockouts as well.
        public OngoingTradeResponse HandleOngoingProductTradeMessage(
            TradeRepublicProductPriceResponseDto trMessage, PositionType type, int ongoingProductId)
            // TradeRepublicProductPriceResponseDto dto, PositionType positionType, int ongoingProductId)
        {
            var cachedWarrant = _cache.GetCachedValue<OngoingWarrantPosition>(ongoingProductId.ToString());
        
            OngoingWarrantPosition ongoingProduct;
            if (cachedWarrant.Expired)
            {
                using (var db = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<DbSolidTrade>())
                {
                    ongoingProduct = db.OngoingWarrantPositions.Find(ongoingProductId);
                }

                _cache.SetCachedValue(ongoingProduct.Id.ToString(), ongoingProduct);
            }
            else
            {
                ongoingProduct = cachedWarrant.Value;
            }
            
            if (ongoingProduct is null)
                return OngoingTradeResponse.PositionsAlreadyClosed;
        
            var price = Math.Min(trMessage.Ask.Price, ongoingProduct.Price);
            var isFulfilled = GetOngoingProductHandler(ongoingProduct.Type, trMessage, price);
        
            if (!isFulfilled)
                return OngoingTradeResponse.WaitingForFill;
        
            try
            {
                using var database = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<DbSolidTrade>();
                
                ongoingProduct = database.OngoingWarrantPositions
                    .Include(p => p.Portfolio)
                    .FirstOrDefault(p => p.Id == ongoingProductId);
        
                // Double check if the ongoing product is not already closed by the user. There is a chance that the cached value is is not in the database anymore.
                if (ongoingProduct is null)
                   // Product is already close (deleted) by the user.
                    return OngoingTradeResponse.PositionsAlreadyClosed;

                var requiredCapital = ongoingProduct.NumberOfShares * price;
        
                if (requiredCapital > ongoingProduct.Portfolio.Balance)
                {
                    // Todo: User should get alerted if this happens.
                    _logger.Warning(Constants.LogMessageTemplate, new InsufficientFounds
                    {
                        Title = "Not enough buying power",
                        Message = $"User ongoing product was satisfied but had not sufficient founds. User balance: {ongoingProduct.Portfolio.Balance} but required capital is: {requiredCapital}.",
                        AdditionalData = new { ProductId = ongoingProductId, Type = type, },
                    });
                    return OngoingTradeResponse.Failed;
                }
                
                ongoingProduct.CurrentWarrantPosition ??= database.WarrantPositions
                    .AsQueryable()
                    .FirstOrDefault(w => w.Isin == ongoingProduct.Isin && w.Portfolio.Id == ongoingProduct.Portfolio.Id);

                WarrantPosition warrantPosition = ongoingProduct.CurrentWarrantPosition;
                if (warrantPosition is not null)
                {
                    var position = CalculateNewPosition(ongoingProduct.CurrentWarrantPosition, new WarrantPosition
                    {
                        NumberOfShares = ongoingProduct.NumberOfShares, BuyInPrice = price,
                    });

                    warrantPosition.BuyInPrice = position.BuyInPrice;
                    warrantPosition.NumberOfShares = position.NumberOfShares;

                    database.WarrantPositions.Update(warrantPosition);
                }
                else
                {
                    warrantPosition = new WarrantPosition
                    {
                        Isin = ongoingProduct.Isin,
                        Portfolio = ongoingProduct.Portfolio,
                        BuyInPrice = price,
                        NumberOfShares = ongoingProduct.NumberOfShares,
                    };
                    
                    database.WarrantPositions.Add(warrantPosition);
                }

                database.OngoingWarrantPositions.Remove(ongoingProduct);

                // Todo: Add to history trades
                // Todo: Notify user. Also send update via firestore.

                ongoingProduct.Portfolio.Balance -= requiredCapital;

                database.Portfolios.Update(ongoingProduct.Portfolio);
                
                database.SaveChanges();
                return OngoingTradeResponse.Complete;
            }
            catch (Exception e)
            {
                _logger.Error(Constants.LogMessageTemplate, new UnexpectedError
                {
                    Title = "Ongoing trade save failed",
                    Message = "Failed to save fill of ongoing trade",
                    Exception = e,
                    AdditionalData = new
                    {
                        TradeRepubmucMessage = trMessage,
                        OngoingProductId = ongoingProductId,
                        Type = type,
                    },
                });
                return OngoingTradeResponse.Failed;
            }
        }
    }
}
