using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;
using Serilog;
using SolidTradeServer.Data.Common;
using SolidTradeServer.Data.Dtos.Shared.Common;
using SolidTradeServer.Data.Dtos.Stock.Response;
using SolidTradeServer.Data.Dtos.TradeRepublic;
using SolidTradeServer.Data.Entities;
using SolidTradeServer.Data.Models.Enums;
using SolidTradeServer.Data.Models.Errors;
using SolidTradeServer.Data.Models.Errors.Common;
using SolidTradeServer.Services.TradeRepublic;
using static SolidTradeServer.Common.Shared;
using NotFound = SolidTradeServer.Data.Models.Errors.NotFound;

namespace SolidTradeServer.Services
{
    public class StockService
    {
        private readonly ILogger _logger = Log.ForContext<StockService>();
        
        private readonly TradeRepublicApiService _trApiService;
        private readonly DbSolidTrade _database;
        private readonly IMapper _mapper;

        public StockService(DbSolidTrade database, IMapper mapper, TradeRepublicApiService trApiService)
        {
            _trApiService = trApiService;
            _database = database;
            _mapper = mapper;
        }

        public async Task<OneOf<StockPositionResponseDto, ErrorResponse>> GetStock(int id, string uid)
        {
            var user = await _database.Users.AsQueryable()
                .FirstOrDefaultAsync(u => u.Portfolio.StockPositions.Any(sp => sp.Id == id));

            if (user is null)
            {
                return new ErrorResponse(new NotFound
                {
                    Title = "User not found",
                    Message = $"User with uid: {uid} could not be found or does not own stock with id: {id}.",
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

            var stock = await _database.StockPositions.FindAsync(id);

            if (stock is null)
            {
                return new ErrorResponse(new NotFound
                {
                    Title = "Stock not found",
                    Message = $"Stock with id: {id} could not be found",
                }, HttpStatusCode.NotFound);
            }
            
            _logger.Information("User with user uid {@Uid} fetched stock with stock id {@StockId} successfully", uid, id);

            return _mapper.Map<StockPositionResponseDto>(stock);
        }

        public async Task<OneOf<StockPositionResponseDto, ErrorResponse>> BuyStock(BuyOrSellRequestDto dto, string uid)
        {
            var result = await _trApiService.ValidateRequest(dto.Isin);

            if (result.TryPickT1(out var errorResponse, out _))
                return errorResponse;

            if ((await _trApiService.MakeTrRequest<TradeRepublicProductPriceResponseDto>(GetTradeRepublicProductPriceRequestString(dto.Isin))).TryPickT1(
                out errorResponse, out var trResponse))
                return errorResponse;

            var user = await _database.Users
                .Include(u => u.Portfolio)
                .FirstOrDefaultAsync(u => u.Uid == uid);
            
            var totalPrice = trResponse.Ask.Price * dto.NumberOfShares;

            if (totalPrice > user.Portfolio.Cash)
            {
                return new ErrorResponse(new InsufficientFounds
                {
                    Title = "Insufficient funds",
                    Message = "User founds not sufficient for purchase.",
                    UserFriendlyMessage = $"Balance insufficient. The total price is {totalPrice} but you have a balance of {user.Portfolio.Cash}",
                    AdditionalData = new
                    {
                        TotalPrice = totalPrice, UserBalance = user.Portfolio.Cash, Dto = dto,
                    },
                }, HttpStatusCode.PaymentRequired);
            }
            
            var stock = new StockPosition
            {
                Isin = ToIsinWithoutExchangeExtension(dto.Isin),
                BuyInPrice = trResponse.Ask.Price,
                Portfolio = user.Portfolio,
                NumberOfShares = dto.NumberOfShares,
            };
            
            var historicalPositions = new HistoricalPosition
            {
                BuyOrSell = BuyOrSell.Buy,
                Isin = stock.Isin,
                Performance = -1,
                PositionType = PositionType.Stock,
                UserId = user.Id,
                BuyInPrice = trResponse.Ask.Price,
                NumberOfShares = dto.NumberOfShares,
            };

            var (isNew, newStock) = await AddOrUpdate(stock, user.Portfolio.Id);

            try
            {
                if (isNew)
                    newStock = _database.StockPositions.Add(newStock).Entity;
                else
                    newStock = _database.StockPositions.Update(newStock).Entity;

                user.Portfolio.Cash -= totalPrice;

                _database.Portfolios.Update(user.Portfolio);
                _database.HistoricalPositions.Add(historicalPositions);
                
                _logger.Information("Trying to save buy stock with isin {@Isin} for User with uid {@Uid}", dto.Isin, uid);
                await _database.SaveChangesAsync();
                _logger.Information("Save buy stock with isin {@Isin} for User with uid {@Uid} was successful", dto.Isin, uid);
                return _mapper.Map<StockPositionResponseDto>(newStock);
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
        
        public async Task<OneOf<StockPositionResponseDto, ErrorResponse>> SellStock(BuyOrSellRequestDto dto, string uid)
        {
            var result = await _trApiService.ValidateRequest(dto.Isin);

            if (result.TryPickT1(out var errorResponse, out _))
                return errorResponse;

            var isinWithoutExchangeExtension = ToIsinWithoutExchangeExtension(dto.Isin);

            if ((await _trApiService.MakeTrRequest<TradeRepublicProductPriceResponseDto>(GetTradeRepublicProductPriceRequestString(dto.Isin))).TryPickT1(
                out errorResponse, out var trResponse))
                return errorResponse;

            var user = await _database.Users
                .Include(u => u.Portfolio)
                .FirstOrDefaultAsync(u => u.Uid == uid);

            var totalGain = trResponse.Bid.Price * dto.NumberOfShares;

            var stockPosition = await _database.StockPositions.AsQueryable()
                .FirstOrDefaultAsync(w =>
                    EF.Functions.Like(w.Isin, $"%{isinWithoutExchangeExtension}%") && user.Portfolio.Id == w.Portfolio.Id);
            
            if (stockPosition is null)
            {
                return new ErrorResponse(new NotFound
                {
                    Title = "Stock not found",
                    Message = $"Stock with isin: {ToIsinWithoutExchangeExtension(dto.Isin)} could not be found.",
                    AdditionalData = new { Dto = dto }
                }, HttpStatusCode.NotFound);
            }

            if (stockPosition.NumberOfShares < dto.NumberOfShares)
            {
                return new ErrorResponse(new TradeFailed
                {
                    Title = "Sell failed",
                    Message = "Can't sell more shares than existent",
                    UserFriendlyMessage = "You can't sell more shares than you have.",
                    AdditionalData = new { Dto = dto, Stock = _mapper.Map<StockPositionResponseDto>(stockPosition) }
                }, HttpStatusCode.BadRequest);
            }
            
            var performance = trResponse.Bid.Price / stockPosition.BuyInPrice;
            
            var historicalPositions = new HistoricalPosition
            {
                BuyOrSell = BuyOrSell.Sell,
                Isin = isinWithoutExchangeExtension,
                Performance = performance,
                PositionType = PositionType.Stock,
                UserId = user.Id,
                BuyInPrice = trResponse.Bid.Price,
                NumberOfShares = dto.NumberOfShares,
            };

            try
            {
                user.Portfolio.Cash += totalGain;
                
                if (stockPosition.NumberOfShares == dto.NumberOfShares)
                    _database.StockPositions.Remove(stockPosition);
                else
                {
                    stockPosition.NumberOfShares -= dto.NumberOfShares;
                    _database.StockPositions.Update(stockPosition);
                }

                _database.Portfolios.Update(user.Portfolio);
                _database.HistoricalPositions.Add(historicalPositions);
                
                _logger.Information("Trying to save sell stock with isin {@Isin} for User with uid {@Uid}", dto.Isin, uid);
                await _database.SaveChangesAsync();
                _logger.Information("Save sell stock with isin {@Isin} for User with uid {@Uid} was successful", dto.Isin, uid);
                return _mapper.Map<StockPositionResponseDto>(stockPosition);
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
                        SoldAll = stockPosition.NumberOfShares == dto.NumberOfShares, Dto = dto, UserUid = uid,
                        Message = "Maybe there was a problem with the isin?"
                    },
                }, HttpStatusCode.InternalServerError);
            }
        }

        private async Task<(bool, StockPosition)> AddOrUpdate(StockPosition stockPosition, int portfolioId)
        {
            var stock = await _database.StockPositions.AsQueryable()
                .FirstOrDefaultAsync(w =>
                    EF.Functions.Like(w.Isin, $"%{stockPosition.Isin}%") && portfolioId == w.Portfolio.Id);

            if (stock is null)
                return (true, stockPosition);

            var position = CalculateNewPosition(stockPosition, stock);

            stock.BuyInPrice = position.BuyInPrice;
            stock.NumberOfShares = position.NumberOfShares;
            
            return (false, stock);
        }
    }
}