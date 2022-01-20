using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;
using SolidTradeServer.Common;
using SolidTradeServer.Data.Common;
using SolidTradeServer.Data.Dtos.Knockout.Response;
using SolidTradeServer.Data.Dtos.Shared.Common;
using SolidTradeServer.Data.Dtos.TradeRepublic;
using SolidTradeServer.Data.Entities;
using SolidTradeServer.Data.Models.Enums;
using SolidTradeServer.Data.Models.Errors;
using SolidTradeServer.Data.Models.Errors.Common;
using SolidTradeServer.Services.Common;
using NotFound = SolidTradeServer.Data.Models.Errors.NotFound;

namespace SolidTradeServer.Services
{
    public class KnockoutService
    {
        private readonly TradeRepublicApiService _trApiService;
        private readonly DbSolidTrade _database;
        private readonly IMapper _mapper;

        public KnockoutService(DbSolidTrade database, IMapper mapper, TradeRepublicApiService trApiService)
        {
            _trApiService = trApiService;
            _database = database;
            _mapper = mapper;
        }

        public async Task<OneOf<KnockoutPositionResponseDto, ErrorResponse>> GetKnockout(int id, string uid)
        {
            var user = await _database.Users.AsQueryable()
                .FirstOrDefaultAsync(u => u.Portfolio.KnockOutPositions.Any(w => w.Id == id));

            if (user is null)
            {
                return new ErrorResponse(new NotFound
                {
                    Title = "User not found",
                    Message = $"User with uid: {uid} could not be found",
                }, HttpStatusCode.NotFound);
            }
            
            if (!user.HasPublicPortfolio && uid != user.Uid)
            {
                return new ErrorResponse(new NotAuthorized
                {
                    Title = "Portfolio is private",
                    Message = "Tried to access other user's portfolio",
                }, HttpStatusCode.Unauthorized);
            }

            var knockoutPosition = await _database.KnockoutPositions.FindAsync(id);

            if (knockoutPosition is null)
            {
                return new ErrorResponse(new NotFound
                {
                    Title = "Knockout not found",
                    Message = $"Knockout with id: {id} could not be found.",
                }, HttpStatusCode.NotFound);
            }

            return _mapper.Map<KnockoutPositionResponseDto>(knockoutPosition);
        }

        public async Task<OneOf<KnockoutPositionResponseDto, ErrorResponse>> BuyKnockout(BuyOrSellRequestDto dto, string uid)
        {
            if ((await IsStockMarketOpen(dto)).TryPickT1(out var errorResponse1, out _))
                return errorResponse1;
            
            if ((await MakeTrRequest<TradeRepublicProductInfoDto>(Constants.GetTradeRepublicProductInfoRequestString(CommonService.CleanIsin(dto.Isin)), dto)).TryPickT1(
                out var errorResponse2, out var productInfo))
                return errorResponse2;

            if (productInfo.DerivativeInfo.ProductCategoryName is ProductCategory.Turbo)
            {
                const string message = "Product is not Open End Turbo. Only Open End Turbo knockouts can be traded.";
                return new ErrorResponse(new TradeFailed
                {
                    Title = "Product is not Open End Turbo",
                    Message = message,
                    UserFriendlyMessage = message,
                    AdditionalData = new {Dto = dto}
                }, HttpStatusCode.BadRequest);
            }
            
            if (!productInfo.Active!.Value)
            {
                const string message = "Product can not be bought or sold. This might happen if the product is expired or is knocked out.";
                return new ErrorResponse(new TradeFailed
                {
                    Title = "Product can not be traded",
                    Message = message,
                    UserFriendlyMessage = message,
                    AdditionalData = new {Dto = dto}
                }, HttpStatusCode.BadRequest);
            }
            
            if ((await MakeTrRequest<TradeRepublicProductPriceResponseDto>(Constants.GetTradeRepublicProductPriceRequestString(dto.Isin), dto)).TryPickT1(
                out var errorResponse3, out var trResponse))
                return errorResponse3;

            var user = await _database.Users
                .Include(u => u.Portfolio)
                .FirstOrDefaultAsync(u => u.Uid == uid);
            
            var totalPrice = trResponse.Ask.Price * dto.NumberOfShares;

            if (totalPrice > user.Portfolio.Balance)
            {
                return new ErrorResponse(new InsufficientFounds
                {
                    Title = "Insufficient funds",
                    Message = "User founds not sufficient for purchase.",
                    UserFriendlyMessage =
                        $"Balance insufficient. The total price is {totalPrice} but you have a balance of {user.Portfolio.Balance}",
                    AdditionalData = new
                    {
                        TotalPrice = totalPrice, UserBalance = user.Portfolio.Balance, Dto = dto,
                    },
                }, HttpStatusCode.PaymentRequired);
            }
            
            var knockout = new KnockoutPosition
            {
                Isin = CommonService.CleanIsin(dto.Isin),
                BuyInPrice = trResponse.Ask.Price,
                Portfolio = user.Portfolio,
                NumberOfShares = dto.NumberOfShares,
            };
            
            var historicalPositions = new HistoricalPosition
            {
                BuyOrSell = BuyOrSell.Buy,
                Isin = knockout.Isin,
                Performance = -1,
                PositionType = PositionType.Knockout,
                UserId = user.Id,
                BuyInPrice = trResponse.Ask.Price,
                NumberOfShares = dto.NumberOfShares,
            };

            var (isNew, newKnockout) = await AddOrUpdate(knockout, user.Portfolio.Id);

            try
            {
                if (isNew)
                    newKnockout = _database.KnockoutPositions.Add(newKnockout).Entity;
                else
                    newKnockout = _database.KnockoutPositions.Update(newKnockout).Entity;

                user.Portfolio.Balance -= totalPrice;

                _database.Portfolios.Update(user.Portfolio);
                _database.HistoricalPositions.Add(historicalPositions);
                
                await _database.SaveChangesAsync();
                return _mapper.Map<KnockoutPositionResponseDto>(newKnockout);
            }
            catch (Exception e)
            {
                return new ErrorResponse(new UnexpectedError
                {
                    Title = "Could not buy position",
                    Message = "Failed to buy position.",
                    Exception = e,
                    UserFriendlyMessage = "Something went very wrong. Please try again later.",
                    AdditionalData = new { IsNew = isNew, Dto = dto, UserUid = uid, Message = "Maybe there was a problem with the isin?" },
                }, HttpStatusCode.InternalServerError);
            }
        }
        
        public async Task<OneOf<KnockoutPositionResponseDto, ErrorResponse>> SellKnockout(BuyOrSellRequestDto dto, string uid)
        {
            if ((await IsStockMarketOpen(dto)).TryPickT1(out var errorResponse1, out _))
                return errorResponse1;

            var cleanIsin = CommonService.CleanIsin(dto.Isin);

            if ((await MakeTrRequest<TradeRepublicProductInfoDto>(Constants.GetTradeRepublicProductInfoRequestString(cleanIsin), dto)).TryPickT1(
                out var errorResponse2, out var isActiveResponse))
                return errorResponse2;

            if (!isActiveResponse.Active!.Value)
            {
                const string message = "Product can not be bought or sold. This might happen if the product is expired or is knocked out.";
                return new ErrorResponse(new TradeFailed
                {
                    Title = "Product can not be traded",
                    Message = message,
                    UserFriendlyMessage = message,
                    AdditionalData = new {Dto = dto}
                }, HttpStatusCode.BadRequest);
            }

            if ((await MakeTrRequest<TradeRepublicProductPriceResponseDto>(Constants.GetTradeRepublicProductPriceRequestString(dto.Isin), dto)).TryPickT1(
                out var errorResponse3, out var trResponse))
                return errorResponse3;

            var user = await _database.Users
                .Include(u => u.Portfolio)
                .FirstOrDefaultAsync(u => u.Uid == uid);

            var totalGain = trResponse.Bid.Price * dto.NumberOfShares;

            var knockoutPosition = await _database.KnockoutPositions.AsQueryable()
                .FirstOrDefaultAsync(w =>
                    EF.Functions.Like(w.Isin, $"%{cleanIsin}%") && user.Portfolio.Id == w.Portfolio.Id);
            
            if (knockoutPosition is null)
            {
                return new ErrorResponse(new NotFound
                {
                    Title = "Knockout not found",
                    Message = $"Knockout with isin: {CommonService.CleanIsin(dto.Isin)} could not be found.",
                    AdditionalData = new { Dto = dto }
                }, HttpStatusCode.NotFound);
            }

            if (knockoutPosition.NumberOfShares < dto.NumberOfShares)
            {
                return new ErrorResponse(new TradeFailed
                {
                    Title = "Sell failed",
                    Message = "Can't sell more shares than existent",
                    UserFriendlyMessage = "You can't sell more shares than you have.",
                    AdditionalData = new { Dto = dto, Knockout = _mapper.Map<KnockoutPositionResponseDto>(knockoutPosition) }
                }, HttpStatusCode.BadRequest);
            }
            
            var performance = trResponse.Bid.Price / knockoutPosition.BuyInPrice;
            
            var historicalPositions = new HistoricalPosition
            {
                BuyOrSell = BuyOrSell.Sell,
                Isin = cleanIsin,
                Performance = performance,
                PositionType = PositionType.Knockout,
                UserId = user.Id,
                BuyInPrice = trResponse.Bid.Price,
                NumberOfShares = dto.NumberOfShares,
            };

            try
            {
                user.Portfolio.Balance += totalGain;
                
                if (knockoutPosition.NumberOfShares == dto.NumberOfShares)
                    _database.KnockoutPositions.Remove(knockoutPosition);
                else
                {
                    knockoutPosition.NumberOfShares -= dto.NumberOfShares;
                    _database.KnockoutPositions.Update(knockoutPosition);
                }

                _database.Portfolios.Update(user.Portfolio);
                _database.HistoricalPositions.Add(historicalPositions);
                
                await _database.SaveChangesAsync();
                return _mapper.Map<KnockoutPositionResponseDto>(knockoutPosition);
            }
            catch (Exception e)
            {
                return new ErrorResponse(new UnexpectedError
                {
                    Title = "Could not sell position",
                    Message = "Failed to sell position.",
                    Exception = e,
                    UserFriendlyMessage = "Something went very wrong. Please try again later.",
                    AdditionalData = new
                    {
                        SoldAll = knockoutPosition.NumberOfShares == dto.NumberOfShares, Dto = dto, UserUid = uid,
                        Message = "Maybe there was a problem with the isin?"
                    },
                }, HttpStatusCode.InternalServerError);
            }
        }

        private async Task<OneOf<T, ErrorResponse>> MakeTrRequest<T>(string requestString, BuyOrSellRequestDto dto)
        {
            var cts = new CancellationTokenSource();
            T trResponse;
            
            try
            {
                cts.CancelAfter(1000 * 8);
                var oneOfResult =
                    await _trApiService.AddRequest<T>(requestString, cts.Token);

                if (oneOfResult.TryPickT1(out var error, out trResponse))
                    return new ErrorResponse(error, HttpStatusCode.InternalServerError);
            }
            catch (OperationCanceledException e)
            {
                return new ErrorResponse(new UnexpectedError
                {
                    Title = "Task timeout",
                    Message = "Fetching product using trade republic api took too long.",
                    AdditionalData = new {dto},
                    Exception = e,
                }, HttpStatusCode.InternalServerError);
            }
            finally { cts.Dispose(); }

            return trResponse;
        }

        private async Task<OneOf<Success, ErrorResponse>> IsStockMarketOpen(BuyOrSellRequestDto dto)
        {
            if ((await _trApiService.IsStockMarketOpen(dto.Isin)).TryPickT1(out var unexpectedError, out var isStockMarketOpen))
                return new ErrorResponse(unexpectedError, HttpStatusCode.InternalServerError);

            if (!isStockMarketOpen)
            {
                return new ErrorResponse(new StockMarketClosed
                {
                    Title = "Stock market closed",
                    Message = "Tried to trade while stock market was closed.",
                    UserFriendlyMessage = "The stock market is unfortunately already closed.",
                }, HttpStatusCode.FailedDependency);
            }

            return new Success();
        }

        private async Task<(bool, KnockoutPosition)> AddOrUpdate(KnockoutPosition knockoutPosition, int portfolioId)
        {
            var knockout = await _database.KnockoutPositions.AsQueryable()
                .FirstOrDefaultAsync(w =>
                    EF.Functions.Like(w.Isin, $"%{knockoutPosition.Isin}%") && portfolioId == w.Portfolio.Id);

            if (knockout is null)
                return (true, knockoutPosition);

            var position = CommonService.CalculateNewPosition(knockoutPosition, knockout);

            knockout.BuyInPrice = position.BuyInPrice;
            knockout.NumberOfShares = position.NumberOfShares;
            
            return (false, knockout);
        }
    }
}