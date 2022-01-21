using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OneOf;
using Serilog;
using SolidTradeServer.Data.Common;
using SolidTradeServer.Data.Dtos.Knockout.Response;
using SolidTradeServer.Data.Dtos.Shared.Common;
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
    public class KnockoutService
    {
        private readonly ILogger _logger = Log.ForContext<KnockoutService>();
        
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
            
            _logger.Information("User with user uid {@Uid} fetched knockout with knockout id {@KnockoutId} successfully", uid, id);

            return _mapper.Map<KnockoutPositionResponseDto>(knockoutPosition);
        }

        public async Task<OneOf<KnockoutPositionResponseDto, ErrorResponse>> BuyKnockout(BuyOrSellRequestDto dto, string uid)
        {
            var result = await _trApiService.ValidateRequest(dto.Isin);

            if (result.TryPickT1(out var errorResponse, out var productInfo))
                return errorResponse;

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
            
            if ((await _trApiService.MakeTrRequest<TradeRepublicProductPriceResponseDto>(GetTradeRepublicProductPriceRequestString(dto.Isin))).TryPickT1(
                out var errorResponse3, out var trResponse))
                return errorResponse3;

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
                    UserFriendlyMessage =
                        $"Balance insufficient. The total price is {totalPrice} but you have a balance of {user.Portfolio.Cash}",
                    AdditionalData = new
                    {
                        TotalPrice = totalPrice, UserBalance = user.Portfolio.Cash, Dto = dto,
                    },
                }, HttpStatusCode.PaymentRequired);
            }
            
            var knockout = new KnockoutPosition
            {
                Isin = ToIsinWithoutExchangeExtension(dto.Isin),
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

                user.Portfolio.Cash -= totalPrice;

                _database.Portfolios.Update(user.Portfolio);
                _database.HistoricalPositions.Add(historicalPositions);
                
                _logger.Information("Trying to save buy knockout with isin {@Isin} for User with uid {@Uid}", dto.Isin, uid);
                await _database.SaveChangesAsync();
                _logger.Information("Save buy knockout with isin {@Isin} for User with uid {@Uid} was successful", dto.Isin, uid);
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

            var knockoutPosition = await _database.KnockoutPositions.AsQueryable()
                .FirstOrDefaultAsync(w =>
                    EF.Functions.Like(w.Isin, $"%{isinWithoutExchangeExtension}%") && user.Portfolio.Id == w.Portfolio.Id);
            
            if (knockoutPosition is null)
            {
                return new ErrorResponse(new NotFound
                {
                    Title = "Knockout not found",
                    Message = $"Knockout with isin: {isinWithoutExchangeExtension} could not be found.",
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
                Isin = isinWithoutExchangeExtension,
                Performance = performance,
                PositionType = PositionType.Knockout,
                UserId = user.Id,
                BuyInPrice = trResponse.Bid.Price,
                NumberOfShares = dto.NumberOfShares,
            };

            try
            {
                user.Portfolio.Cash += totalGain;
                
                if (knockoutPosition.NumberOfShares == dto.NumberOfShares)
                    _database.KnockoutPositions.Remove(knockoutPosition);
                else
                {
                    knockoutPosition.NumberOfShares -= dto.NumberOfShares;
                    _database.KnockoutPositions.Update(knockoutPosition);
                }

                _database.Portfolios.Update(user.Portfolio);
                _database.HistoricalPositions.Add(historicalPositions);
                
                _logger.Information("Trying to save sell knockout with isin {@Isin} for User with uid {@Uid}", dto.Isin, uid);
                await _database.SaveChangesAsync();
                _logger.Information("Save sell knockout with isin {@Isin} for User with uid {@Uid} was successful", dto.Isin, uid);
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

        private async Task<(bool, KnockoutPosition)> AddOrUpdate(KnockoutPosition knockoutPosition, int portfolioId)
        {
            var knockout = await _database.KnockoutPositions.AsQueryable()
                .FirstOrDefaultAsync(w =>
                    EF.Functions.Like(w.Isin, $"%{knockoutPosition.Isin}%") && portfolioId == w.Portfolio.Id);

            if (knockout is null)
                return (true, knockoutPosition);

            var position = CalculateNewPosition(knockoutPosition, knockout);

            knockout.BuyInPrice = position.BuyInPrice;
            knockout.NumberOfShares = position.NumberOfShares;
            
            return (false, knockout);
        }
    }
}