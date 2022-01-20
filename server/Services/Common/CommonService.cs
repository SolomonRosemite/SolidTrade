using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OneOf;
using Serilog;
using SolidTradeServer.Data.Common;
using SolidTradeServer.Data.Dtos.TradeRepublic;
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
        private readonly NotificationService _notificationService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ICacheService _cache;

        public CommonService(NotificationService notificationService, ICacheService cache, IServiceScopeFactory scopeFactory)
        {
            _notificationService = notificationService;
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

        public OngoingTradeResponse HandleOngoingWarrantTradeMessage(TradeRepublicApiService trService, 
            TradeRepublicProductPriceResponseDto trMessage, PositionType type, int ongoingProductId)
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
            
            if (ongoingProduct is null || DateTimeOffset.Now > ongoingProduct.GoodUntil)
                return OngoingTradeResponse.PositionsAlreadyClosed;
            
            decimal price;
            var isBuyOrSell = IsBuyOrSell(ongoingProduct.Type);
            
            if (isBuyOrSell == BuyOrSell.Buy)
                price = Math.Min(trMessage.Ask.Price, ongoingProduct.Price);
            else
                price = Math.Max(trMessage.Bid.Price, ongoingProduct.Price);
            
            var isFulfilled = GetOngoingProductHandler(ongoingProduct.Type, trMessage, ongoingProduct.Price);
        
            if (!isFulfilled)
                return OngoingTradeResponse.WaitingForFill;
        
            OneOf<TradeRepublicProductInfoDto, ErrorResponse> oneOfResult;

            try
            {
                oneOfResult = MakeTrRequest<TradeRepublicProductInfoDto>(trService, 
                    Constants.GetTradeRepublicProductInfoRequestString(ongoingProduct.Isin)).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                _logger.Error(Constants.LogMessageTemplate, new UnexpectedError
                {
                    Title = "Unable to make Trade Republic request",
                    Message = "Unexpect error when trying to make trade republic request.",
                    AdditionalData = new { Isin = ongoingProduct.Isin },
                    Exception = e,
                });
                return OngoingTradeResponse.WaitingForFill;
            }
            
            if (oneOfResult.TryPickT1(out var errorResponse, out var isActiveResponse))
            {
                _logger.Error(Constants.LogMessageTemplate, errorResponse);
                return OngoingTradeResponse.WaitingForFill;
            }

            if (!isActiveResponse.Active!.Value)
            {
                // Todo: Notify user.
                const string message = "Ongoing product can not be bought or sold. This might happen if the product is expired or is knocked out.";
                var err = new TradeFailed
                {
                    Title = "Product can not be traded",
                    Message = message,
                    UserFriendlyMessage = message,
                    AdditionalData = new { Dto = ongoingProduct.Isin }
                };
                _logger.Error(Constants.LogMessageTemplate, err);
                return OngoingTradeResponse.Failed;
            }
            
            try
            {
                using var database = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<DbSolidTrade>();
                
                ongoingProduct = database.OngoingWarrantPositions
                    .Include(p => p.Portfolio)
                    .FirstOrDefault(p => p.Id == ongoingProductId);
        
                // Double check if the ongoing product is not already closed by the user. There is a chance that the cached value is not in the database anymore.
                if (ongoingProduct is null)
                   // Product is already closed by the user.
                    return OngoingTradeResponse.PositionsAlreadyClosed;
                
                var totalPrice = ongoingProduct.NumberOfShares * price;
                HistoricalPosition historicalPosition;
                
                if (isBuyOrSell is BuyOrSell.Buy)
                {
                    if (totalPrice > ongoingProduct.Portfolio.Balance)
                    {
                        var message =
                            $"User ongoing product was satisfied but had not sufficient founds. User balance: {ongoingProduct.Portfolio.Balance} but required capital is: {totalPrice}.";
                        
                        _notificationService.SendNotification(ongoingProduct.Portfolio.UserId, "", "Position could not be filled",
                            $"Your {GetOrderName(ongoingProduct.Type)} order could not be executed. {message}");

                        database.OngoingWarrantPositions.Remove(ongoingProduct);
                        
                        _logger.Warning(Constants.LogMessageTemplate, new InsufficientFounds
                        {
                            Title = "Not enough buying power",
                            Message = message,
                            AdditionalData = new { ProductId = ongoingProductId, Type = type, },
                        });

                        database.SaveChanges();
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
                    
                    historicalPosition = new HistoricalPosition
                    {
                        BuyOrSell = isBuyOrSell,
                        Isin = ongoingProduct.Isin,
                        Performance = -1,
                        PositionType = PositionType.Warrant,
                        UserId = ongoingProduct.Portfolio.UserId,
                        BuyInPrice = price,
                        NumberOfShares = ongoingProduct.NumberOfShares,
                    };
                }
                else
                {
                    ongoingProduct.CurrentWarrantPosition ??= database.WarrantPositions
                        .AsQueryable()
                        .FirstOrDefault(w => w.Isin == ongoingProduct.Isin && w.Portfolio.Id == ongoingProduct.Portfolio.Id);

                    WarrantPosition warrantPosition = ongoingProduct.CurrentWarrantPosition;

                    if (warrantPosition is null || warrantPosition.NumberOfShares < ongoingProduct.NumberOfShares)
                    {
                        var orderName = $"{GetOrderName(ongoingProduct.Type)} order";
                        const string message = "Order not executed because warrant does not exist anymore or the number of shares are less then the tried to sell.";

                        _notificationService.SendNotification(ongoingProduct.Portfolio.UserId, "", "Position could not be filled",
                            $"Your {orderName} could not be executed. {message}");
                        
                        _logger.Warning(Constants.LogMessageTemplate, new UnexpectedError
                        {
                            Title = "Could not fill position",
                            Message = message,
                        });
                        
                        database.OngoingWarrantPositions.Remove(ongoingProduct);
                        database.SaveChanges();
                        return OngoingTradeResponse.Failed;
                    }
                    
                    warrantPosition.NumberOfShares -= ongoingProduct.NumberOfShares;
                    
                    historicalPosition = new HistoricalPosition
                    {
                        BuyOrSell = isBuyOrSell,
                        Isin = ongoingProduct.Isin,
                        Performance = price / warrantPosition.BuyInPrice,
                        PositionType = PositionType.Warrant,
                        UserId = ongoingProduct.Portfolio.UserId,
                        BuyInPrice = price,
                        NumberOfShares = ongoingProduct.NumberOfShares,
                    };
                    
                    if (warrantPosition.NumberOfShares == 0)
                        database.WarrantPositions.Remove(warrantPosition);
                    else
                        database.WarrantPositions.Update(warrantPosition);
                }

                database.OngoingWarrantPositions.Remove(ongoingProduct);
                
                // Todo: Also send update via firestore.
                _notificationService.SendNotification(ongoingProduct.Portfolio.UserId, "", "Position filled", $"Your {GetOrderName(ongoingProduct.Type)} order was executed.");

                if (isBuyOrSell is BuyOrSell.Buy)
                    ongoingProduct.Portfolio.Balance -= totalPrice;
                else 
                    ongoingProduct.Portfolio.Balance += totalPrice;

                database.Portfolios.Update(ongoingProduct.Portfolio);
                database.HistoricalPositions.Add(historicalPosition);
                
                database.SaveChanges();
                return OngoingTradeResponse.Complete;
            }
            catch (Exception e)
            {
                _logger.Error(Constants.LogMessageTemplate, new UnexpectedError
                {
                    Title = "Ongoing trade update failed",
                    Message = "Failed to process fill of ongoing trade",
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

        public OngoingTradeResponse HandleOngoingKnockoutTradeMessage(TradeRepublicApiService trService, 
            TradeRepublicProductPriceResponseDto trMessage, PositionType type, int ongoingProductId)
        {
            var cachedKnockout = _cache.GetCachedValue<OngoingKnockoutPosition>(ongoingProductId.ToString());
        
            OngoingKnockoutPosition ongoingProduct;
            if (cachedKnockout.Expired)
            {
                using (var db = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<DbSolidTrade>())
                {
                    ongoingProduct = db.OngoingKnockoutPositions.Find(ongoingProductId);
                }

                _cache.SetCachedValue(ongoingProduct.Id.ToString(), ongoingProduct);
            }
            else
            {
                ongoingProduct = cachedKnockout.Value;
            }
            
            if (ongoingProduct is null || DateTimeOffset.Now > ongoingProduct.GoodUntil)
                return OngoingTradeResponse.PositionsAlreadyClosed;
            
            decimal price;
            var isBuyOrSell = IsBuyOrSell(ongoingProduct.Type);
            
            if (isBuyOrSell == BuyOrSell.Buy)
                price = Math.Min(trMessage.Ask.Price, ongoingProduct.Price);
            else
                price = Math.Max(trMessage.Bid.Price, ongoingProduct.Price);
            
            var isFulfilled = GetOngoingProductHandler(ongoingProduct.Type, trMessage, ongoingProduct.Price);
        
            if (!isFulfilled)
                return OngoingTradeResponse.WaitingForFill;
        
            try
            {
                using var database = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<DbSolidTrade>();
                
                ongoingProduct = database.OngoingKnockoutPositions
                    .Include(p => p.Portfolio)
                    .FirstOrDefault(p => p.Id == ongoingProductId);
        
                // Double check if the ongoing product is not already closed by the user. There is a chance that the cached value is not in the database anymore.
                if (ongoingProduct is null)
                   // Product is already closed by the user.
                    return OngoingTradeResponse.PositionsAlreadyClosed;

                OneOf<TradeRepublicProductInfoDto, ErrorResponse> oneOfResult;

                try
                {
                    oneOfResult = MakeTrRequest<TradeRepublicProductInfoDto>(trService,
                        Constants.GetTradeRepublicProductInfoRequestString(ongoingProduct.Isin)).GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    _logger.Error(Constants.LogMessageTemplate, new UnexpectedError
                    {
                        Title = "Unable to make Trade Republic request",
                        Message = "Unexpect error when trying to make trade republic request.",
                        AdditionalData = new { Isin = ongoingProduct.Isin },
                        Exception = e,
                    });
                    return OngoingTradeResponse.WaitingForFill;
                }

                if (oneOfResult.TryPickT1(out var errorResponse, out var isActiveResponse))
                {
                    _logger.Error(Constants.LogMessageTemplate, errorResponse);
                    return OngoingTradeResponse.WaitingForFill;
                }

                if (!isActiveResponse.Active!.Value)
                {
                    // Todo: Notify user.
                    const string message = "Ongoing product can not be bought or sold. This might happen if the product is expired or is knocked out.";
                    var err = new TradeFailed
                    {
                        Title = "Product can not be traded",
                        Message = message,
                        UserFriendlyMessage = message,
                        AdditionalData = new { Dto = ongoingProduct.Isin }
                    };
                    _logger.Error(Constants.LogMessageTemplate, err);
                    return OngoingTradeResponse.Failed;
                }
                
                var totalPrice = ongoingProduct.NumberOfShares * price;

                HistoricalPosition historicalPosition;
                if (isBuyOrSell is BuyOrSell.Buy)
                {
                    if (totalPrice > ongoingProduct.Portfolio.Balance)
                    {
                        var message =
                            $"User ongoing product was satisfied but had not sufficient founds. User balance: {ongoingProduct.Portfolio.Balance} but required capital is: {totalPrice}.";
                        
                        _notificationService.SendNotification(ongoingProduct.Portfolio.UserId, "", "Position could not be filled",
                            $"Your {GetOrderName(ongoingProduct.Type)} order could not be executed. {message}");

                        database.OngoingKnockoutPositions.Remove(ongoingProduct);
                        
                        _logger.Warning(Constants.LogMessageTemplate, new InsufficientFounds
                        {
                            Title = "Not enough buying power",
                            Message = message,
                            AdditionalData = new { ProductId = ongoingProductId, Type = type, },
                        });

                        database.SaveChanges();
                        return OngoingTradeResponse.Failed;
                    }
                    
                    ongoingProduct.CurrentKnockoutPosition ??= database.KnockoutPositions
                        .AsQueryable()
                        .FirstOrDefault(w => w.Isin == ongoingProduct.Isin && w.Portfolio.Id == ongoingProduct.Portfolio.Id);

                    KnockoutPosition knockoutPosition = ongoingProduct.CurrentKnockoutPosition;
                    if (knockoutPosition is not null)
                    {
                        var position = CalculateNewPosition(ongoingProduct.CurrentKnockoutPosition, new KnockoutPosition
                        {
                            NumberOfShares = ongoingProduct.NumberOfShares, BuyInPrice = price,
                        });

                        knockoutPosition.BuyInPrice = position.BuyInPrice;
                        knockoutPosition.NumberOfShares = position.NumberOfShares;

                        database.KnockoutPositions.Update(knockoutPosition);
                    }
                    else
                    {
                        knockoutPosition = new KnockoutPosition
                        {
                            Isin = ongoingProduct.Isin,
                            Portfolio = ongoingProduct.Portfolio,
                            BuyInPrice = price,
                            NumberOfShares = ongoingProduct.NumberOfShares,
                        };
                        
                        database.KnockoutPositions.Add(knockoutPosition);
                    }
                    
                    historicalPosition = new HistoricalPosition
                    {
                        BuyOrSell = isBuyOrSell,
                        Isin = ongoingProduct.Isin,
                        Performance = -1,
                        PositionType = PositionType.Knockout,
                        UserId = ongoingProduct.Portfolio.UserId,
                        BuyInPrice = price,
                        NumberOfShares = ongoingProduct.NumberOfShares,
                    };
                }
                else
                {
                    ongoingProduct.CurrentKnockoutPosition ??= database.KnockoutPositions
                        .AsQueryable()
                        .FirstOrDefault(w => w.Isin == ongoingProduct.Isin && w.Portfolio.Id == ongoingProduct.Portfolio.Id);

                    KnockoutPosition knockoutPosition = ongoingProduct.CurrentKnockoutPosition;

                    if (knockoutPosition is null || knockoutPosition.NumberOfShares < ongoingProduct.NumberOfShares)
                    {
                        var orderName = $"{GetOrderName(ongoingProduct.Type)} order";
                        const string message = "Order not executed because knockout does not exist anymore or the number of shares are less then the tried to sell.";

                        _notificationService.SendNotification(ongoingProduct.Portfolio.UserId, "", "Position could not be filled",
                            $"Your {orderName} could not be executed. {message}");
                        
                        _logger.Warning(Constants.LogMessageTemplate, new UnexpectedError
                        {
                            Title = "Could not fill position",
                            Message = message,
                        });
                        
                        database.OngoingKnockoutPositions.Remove(ongoingProduct);
                        database.SaveChanges();
                        return OngoingTradeResponse.Failed;
                    }
                    
                    knockoutPosition.NumberOfShares -= ongoingProduct.NumberOfShares;
                    
                    historicalPosition = new HistoricalPosition
                    {
                        BuyOrSell = isBuyOrSell,
                        Isin = ongoingProduct.Isin,
                        Performance = price / knockoutPosition.BuyInPrice,
                        PositionType = PositionType.Knockout,
                        UserId = ongoingProduct.Portfolio.UserId,
                        BuyInPrice = price,
                        NumberOfShares = ongoingProduct.NumberOfShares,
                    };

                    if (knockoutPosition.NumberOfShares == 0)
                        database.KnockoutPositions.Remove(knockoutPosition);
                    else
                        database.KnockoutPositions.Update(knockoutPosition);
                }

                database.OngoingKnockoutPositions.Remove(ongoingProduct);
                
                // Todo: Also send update via firestore.
                _notificationService.SendNotification(ongoingProduct.Portfolio.UserId, "", "Position filled", $"Your {GetOrderName(ongoingProduct.Type)} order was executed.");

                if (isBuyOrSell is BuyOrSell.Buy)
                    ongoingProduct.Portfolio.Balance -= totalPrice;
                else 
                    ongoingProduct.Portfolio.Balance += totalPrice;

                database.Portfolios.Update(ongoingProduct.Portfolio);
                database.HistoricalPositions.Add(historicalPosition);
                
                database.SaveChanges();
                return OngoingTradeResponse.Complete;
            }
            catch (Exception e)
            {
                _logger.Error(Constants.LogMessageTemplate, new UnexpectedError
                {
                    Title = "Ongoing trade update failed",
                    Message = "Failed to process fill of ongoing trade",
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

        private string GetOrderName(EnterOrExitPositionType type)
        {
            return type switch
            {
                EnterOrExitPositionType.BuyLimitOrder => "buy limit",
                EnterOrExitPositionType.BuyStopOrder => "buy stop",
                EnterOrExitPositionType.SellLimitOrder => "take profit",
                EnterOrExitPositionType.SellStopOrder => "stop loss",
            };
        }

        private BuyOrSell IsBuyOrSell(EnterOrExitPositionType type)
        {
            return type switch
            {
                EnterOrExitPositionType.BuyLimitOrder => BuyOrSell.Buy,
                EnterOrExitPositionType.BuyStopOrder => BuyOrSell.Buy,
                EnterOrExitPositionType.SellLimitOrder => BuyOrSell.Sell,
                EnterOrExitPositionType.SellStopOrder => BuyOrSell.Sell,
            };
        }
        
        private static async Task<OneOf<T, ErrorResponse>> MakeTrRequest<T>(TradeRepublicApiService trService, string requestString)
        {
            var cts = new CancellationTokenSource();
            T trResponse;
            
            try
            {
                cts.CancelAfter(1000 * 8);
                var oneOfResult =
                    await trService.AddRequest<T>(requestString, cts.Token);

                if (oneOfResult.TryPickT1(out var error, out trResponse))
                    return new ErrorResponse(error, HttpStatusCode.InternalServerError);
            }
            catch (OperationCanceledException)
            {
                return new ErrorResponse(new UnexpectedError
                {
                    Title = "Task timeout",
                    Message = "Fetching product using trade republic api took too long.",
                    AdditionalData = new { requestString }
                }, HttpStatusCode.InternalServerError);
            }
            finally { cts.Dispose(); }

            return trResponse;
        }
    }
}
