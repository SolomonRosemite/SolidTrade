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
using SolidTradeServer.Data.Dtos.Shared.Common;
using SolidTradeServer.Data.Dtos.TradeRepublic;
using SolidTradeServer.Data.Dtos.Warrant.Response;
using SolidTradeServer.Data.Entities;
using SolidTradeServer.Data.Models.Enums;
using SolidTradeServer.Data.Models.Errors;
using SolidTradeServer.Data.Models.Errors.Common;
using SolidTradeServer.Services.Common;
using NotFound = SolidTradeServer.Data.Models.Errors.NotFound;

namespace SolidTradeServer.Services
{
    public class WarrantService : IDisposable
    {
        private readonly TradeRepublicApiService _trApiService;
        private readonly DbSolidTrade _database;
        private readonly IMapper _mapper;

        public WarrantService(DbSolidTrade database, IMapper mapper, TradeRepublicApiService trApiService)
        {
            _trApiService = trApiService;
            _database = database;
            _mapper = mapper;
        }

        public async Task<OneOf<WarrantPositionResponseDto, ErrorResponse>> GetWarrant(int id, string uid)
        {
            var user = await _database.Users.AsQueryable()
                .FirstOrDefaultAsync(u => u.Portfolio.WarrantPositions.Any(w => w.Id == id));

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

            var warrant = await _database.WarrantPositions.FindAsync(id);

            if (warrant is null)
            {
                return new ErrorResponse(new NotFound
                {
                    Title = "Warrant not found",
                    Message = $"Warrant with id: {id} could not be found",
                }, HttpStatusCode.NotFound);
            }

            return _mapper.Map<WarrantPositionResponseDto>(warrant);
        }

        public async Task<OneOf<WarrantPositionResponseDto, ErrorResponse>> BuyWarrant(BuyOrSellRequestDto dto, string uid)
        {
            var cleanIsin = CommonService.CleanIsin(dto.Isin);
            
            if ((await IsStockMarketOpen(dto)).TryPickT1(out var errorResponse1, out _))
                return errorResponse1;

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
                    AdditionalData = new { Dto = dto }
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
            
            var warrant = new WarrantPosition
            {
                Isin = CommonService.CleanIsin(dto.Isin),
                BuyInPrice = trResponse.Ask.Price,
                Portfolio = user.Portfolio,
                NumberOfShares = dto.NumberOfShares,
            };
            
            var historicalPositions = new HistoricalPosition
            {
                BuyOrSell = BuyOrSell.Buy,
                Isin = warrant.Isin,
                Performance = -1,
                PositionType = PositionType.Warrant,
                UserId = user.Id,
                BuyInPrice = trResponse.Ask.Price,
                NumberOfShares = dto.NumberOfShares,
            };

            var (isNew, newWarrant) = await AddOrUpdate(warrant, user.Portfolio.Id);

            try
            {
                if (isNew)
                    newWarrant = _database.WarrantPositions.Add(newWarrant).Entity;
                else
                    newWarrant = _database.WarrantPositions.Update(newWarrant).Entity;

                user.Portfolio.Balance -= totalPrice;

                _database.Portfolios.Update(user.Portfolio);
                _database.HistoricalPositions.Add(historicalPositions);
                
                await _database.SaveChangesAsync();
                return _mapper.Map<WarrantPositionResponseDto>(newWarrant);
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
        
        public async Task<OneOf<WarrantPositionResponseDto, ErrorResponse>> SellWarrant(BuyOrSellRequestDto dto, string uid)
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
                    AdditionalData = new { Dto = dto }
                }, HttpStatusCode.BadRequest);
            }

            var user = await _database.Users
                .Include(u => u.Portfolio)
                .FirstOrDefaultAsync(u => u.Uid == uid);

            return await SellWarrantInternal(_database, dto, user);
        }

        public async Task<OneOf<WarrantPositionResponseDto, ErrorResponse>> SellWarrantInternal(DbSolidTrade dbSolidTrade, BuyOrSellRequestDto dto, User userWithPortfolio)
        {
            var cleanIsin = CommonService.CleanIsin(dto.Isin);
            
            if ((await MakeTrRequest<TradeRepublicProductPriceResponseDto>(Constants.GetTradeRepublicProductPriceRequestString(dto.Isin), dto)).TryPickT1(
                out var errorResponse3, out var trResponse))
                return errorResponse3;
            
            var totalGain = trResponse.Bid.Price * dto.NumberOfShares;

            var warrantPosition = await dbSolidTrade.WarrantPositions.AsQueryable()
                .FirstOrDefaultAsync(w =>
                    EF.Functions.Like(w.Isin, $"%{cleanIsin}%") && userWithPortfolio.Portfolio.Id == w.Portfolio.Id);

            if (warrantPosition is null)
            {
                return new ErrorResponse(new NotFound
                {
                    Title = "Warrant not found",
                    Message = $"Warrant with isin: {cleanIsin} could not be found.",
                    AdditionalData = new {Dto = dto}
                }, HttpStatusCode.NotFound);
            }

            if (warrantPosition.NumberOfShares < dto.NumberOfShares)
            {
                return new ErrorResponse(new TradeFailed
                {
                    Title = "Sell failed",
                    Message = "Can't sell more shares than existent",
                    UserFriendlyMessage = "You can't sell more shares than you have.",
                    AdditionalData = new {Dto = dto, Warrant = _mapper.Map<WarrantPositionResponseDto>(warrantPosition)}
                }, HttpStatusCode.BadRequest);
            }

            var performance = trResponse.Bid.Price / warrantPosition.BuyInPrice;

            var historicalPositions = new HistoricalPosition
            {
                BuyOrSell = BuyOrSell.Sell,
                Isin = cleanIsin,
                Performance = performance,
                PositionType = PositionType.Warrant,
                UserId = userWithPortfolio.Id,
                BuyInPrice = trResponse.Bid.Price,
                NumberOfShares = dto.NumberOfShares,
            };

            try
            {
                userWithPortfolio.Portfolio.Balance += totalGain;

                if (warrantPosition.NumberOfShares == dto.NumberOfShares)
                    dbSolidTrade.WarrantPositions.Remove(warrantPosition);
                else
                {
                    warrantPosition.NumberOfShares -= dto.NumberOfShares;
                    dbSolidTrade.WarrantPositions.Update(warrantPosition);
                }

                // We save twice here because if we wouldn't the portfolio update would override the warrant position update or remove for some reason.
                // Saving here insures that the portfolio update doesn't effect any warrants thanks to its timestamp.
                await dbSolidTrade.SaveChangesAsync();

                dbSolidTrade.Portfolios.Update(userWithPortfolio.Portfolio);
                dbSolidTrade.HistoricalPositions.Add(historicalPositions);
                await dbSolidTrade.SaveChangesAsync();
                return _mapper.Map<WarrantPositionResponseDto>(warrantPosition);
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
                        SoldAll = warrantPosition.NumberOfShares == dto.NumberOfShares,
                        Dto = dto,
                        UserUid = userWithPortfolio.Uid,
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

        private async Task<(bool, WarrantPosition)> AddOrUpdate(WarrantPosition warrantPosition, int portfolioId)
        {
            var warrant = await _database.WarrantPositions.AsQueryable()
                .FirstOrDefaultAsync(w =>
                    EF.Functions.Like(w.Isin, $"%{warrantPosition.Isin}%") && portfolioId == w.Portfolio.Id);

            if (warrant is null)
                return (true, warrantPosition);

            var position = CommonService.CalculateNewPosition(warrantPosition, warrant);

            warrant.BuyInPrice = position.BuyInPrice;
            warrant.NumberOfShares = position.NumberOfShares;
            
            return (false, warrant);
        }

        public void Dispose()
        {
            _database.Dispose();
        }
    }
}