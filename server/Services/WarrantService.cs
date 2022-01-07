using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;
using SolidTradeServer.Data.Common;
using SolidTradeServer.Data.Dtos.Warrant.Request;
using SolidTradeServer.Data.Dtos.Warrant.Response;
using SolidTradeServer.Data.Dtos.Warrant.TradeRepublic;
using SolidTradeServer.Data.Entities;
using SolidTradeServer.Data.Models.Enums;
using SolidTradeServer.Data.Models.Errors;
using SolidTradeServer.Data.Models.Errors.Common;
using SolidTradeServer.Services.Common;
using NotFound = SolidTradeServer.Data.Models.Errors.NotFound;

namespace SolidTradeServer.Services
{
    public class WarrantService
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
            // Todo: Test if this is too expensive.
            var user = _database.WarrantPositions.FirstOrDefault(w => w.Id == id)?.Portfolio.User;

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

        public async Task<OneOf<WarrantPositionResponseDto, ErrorResponse>> BuyWarrant(BuyWarrantRequestDto dto, string uid)
        {
            if ((await IsStockMarketOpen(dto)).TryPickT1(out var errorResponse1, out _))
                return errorResponse1;

            var requestString = "{\"type\":\"ticker\",\"id\":\"" + dto.WarrantIsin + "\"}";

            if ((await MakeTrRequest<TradeRepublicWarrantResponseDto>(requestString, dto)).TryPickT1(
                out var errorResponse2, out var trResponse))
                return errorResponse2;

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
            
            // Todo: Also for the ongoing order. When to order get a fill check first if the balance is sufficient.
            var warrant = new WarrantPosition
            {
                Isin = CleanIsin(dto.WarrantIsin),
                BuyInPrice = trResponse.Ask.Price,
                Portfolio = user.Portfolio,
                NumberOfShares = dto.NumberOfShares,
            };
            
            var historicalPositions = new HistoricalPosition
            {
                BuyOrSell = BuyOrSell.Buy,
                Isin = warrant.Isin,
                Performance = 0,
                PositionType = PositionType.Warrant,
                UserId = user.Id,
                BuyInPrice = trResponse.Ask.Price,
                NumberOfShares = dto.NumberOfShares,
            };

            var (isNew, newWarrant) = await AddOrUpdate(warrant);

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

        private async Task<OneOf<T, ErrorResponse>> MakeTrRequest<T>(string requestString, BuyWarrantRequestDto dto)
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
            catch (OperationCanceledException)
            {
                return new ErrorResponse(new UnexpectedError
                {
                    Title = "Task timeout",
                    Message = "Fetching warrant using trade republic api took too long.",
                    AdditionalData = new {dto}
                }, HttpStatusCode.InternalServerError);
            }
            finally { cts.Dispose(); }

            return trResponse;
        }

        private async Task<OneOf<Success, ErrorResponse>> IsStockMarketOpen(BuyWarrantRequestDto dto)
        {
            if ((await _trApiService.IsStockMarketOpen(dto.WarrantIsin)).TryPickT1(out var unexpectedError, out var isStockMarketOpen))
                return new ErrorResponse(unexpectedError, HttpStatusCode.InternalServerError);

            if (!isStockMarketOpen)
            {
                return new ErrorResponse(new StockMarketClosed
                {
                    Title = "Stock market closed",
                    Message = "Tried to trade while stock market was closed.",
                    UserFriendlyMessage = "The socket market is unfortunately already closed.",
                }, HttpStatusCode.FailedDependency);
            }

            return new Success();
        }

        // public async Task<OneOf<WarrantPositionResponseDto, ErrorResponse>> SellWarrant(BuyWarrantRequestDto dto, string uid)
        // {
        //     var user = await _database.Users.FirstOrDefaultAsync(u => u.Uid == uid);
        //     
        //     
        // }

        private async Task<(bool, WarrantPosition)> AddOrUpdate(WarrantPosition warrantPosition)
        {
            var warrant = await _database.WarrantPositions.AsQueryable()
                .FirstOrDefaultAsync(w => EF.Functions.Like(w.Isin, $"%{warrantPosition.Isin}%"));

            if (warrant is null)
                return (true, warrantPosition);

            var position = CommonService.CalculateNewPosition(warrantPosition, warrant);

            warrant.BuyInPrice = position.BuyInPrice;
            warrant.NumberOfShares = position.NumberOfShares;
            
            return (false, warrant);
        }

        private string CleanIsin(string isin)
        {
            var i = isin.IndexOf('.');
            return i == -1 ? isin.Trim().ToUpper() : isin.Substring(0, i).Trim().ToUpper();
        }
    }
}