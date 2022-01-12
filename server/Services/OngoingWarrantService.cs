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
using SolidTradeServer.Data.Dtos.OngoingWarrant.Response;
using SolidTradeServer.Data.Dtos.Shared.OngoingPosition.Request;
using SolidTradeServer.Data.Dtos.Warrant.TradeRepublic;
using SolidTradeServer.Data.Entities;
using SolidTradeServer.Data.Models.Enums;
using SolidTradeServer.Data.Models.Errors;
using SolidTradeServer.Data.Models.Errors.Common;
using SolidTradeServer.Services.Common;
using NotFound = SolidTradeServer.Data.Models.Errors.NotFound;

namespace SolidTradeServer.Services
{
    public class OngoingWarrantService
    {
        private readonly TradeRepublicApiService _trApiService;
        private readonly DbSolidTrade _database;
        private readonly IMapper _mapper;

        public OngoingWarrantService(DbSolidTrade database, IMapper mapper, TradeRepublicApiService trApiService)
        {
            _trApiService = trApiService;
            _database = database;
            _mapper = mapper;
        }

        public async Task<OneOf<OngoingWarrantPositionResponseDto, ErrorResponse>> GetOngoingWarrant(int id, string uid)
        {
            // Todo: Test if this is too expensive.
            var user = _database.OngoingWarrantPositions.FirstOrDefault(w => w.Id == id)?.Portfolio.User;

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

            var warrant = await _database.OngoingWarrantPositions.FindAsync(id);

            if (warrant is null)
            {
                return new ErrorResponse(new NotFound
                {
                    Title = "Ongoing warrant not found",
                    Message = $"Ongoing warrant with id: {id} could not be found",
                }, HttpStatusCode.NotFound);
            }

            return _mapper.Map<OngoingWarrantPositionResponseDto>(warrant);
        }
        
        public async Task<OneOf<OngoingWarrantPositionResponseDto, ErrorResponse>> OpenOngoingWarrant(OngoingPositionRequestDto dto, string uid)
        {
            var cleanIsin = CommonService.CleanIsin(dto.Isin);
            var requestString = "{\"type\":\"ticker\",\"id\":\"" + dto.Isin + "\"}";

            if ((await IsStockMarketOpen(dto.Isin)).TryPickT1(out var errorResponse1, out _))
                return errorResponse1;
            
            if ((await MakeTrRequest<TradeRepublicProductPriceResponseDto>(requestString, dto)).TryPickT1(
                out var errorResponse2, out var trResponse))
                return errorResponse2;

            var isFulfilled = CommonService.GetOngoingProductHandler(dto.Type!.Value, trResponse, dto.PriceThreshold);

            if (isFulfilled)
            {
                return new ErrorResponse(new TradeFailed
                {
                    Title = "Invalid trade",
                    Message = "Order price is not appropriate for this order type.",
                    UserFriendlyMessage = "Order price is not appropriate for this order type. Please try again.",
                    AdditionalData = new { Dto = dto, trResponse },
                }, HttpStatusCode.BadRequest);
            }
            
            var user = await _database.Users
                .Include(u => u.Portfolio)
                .FirstOrDefaultAsync(u => u.Uid == uid);

            var existingWarrant = await _database.WarrantPositions.AsQueryable()
                .FirstOrDefaultAsync(w => w.Isin == cleanIsin && w.Portfolio.Id == user.Id);
            
            var ongoingWarrant = new OngoingWarrantPosition
            {
                Isin = cleanIsin,
                Portfolio = user.Portfolio,
                Type = dto.Type!.Value,
                GoodUntil = dto.GoodUntil!.Value,
                CurrentWarrantPosition = existingWarrant,
                NumberOfShares = dto.NumberOfShares,
                Price = dto.PriceThreshold,
            };

            try
            {
                _database.OngoingWarrantPositions.Add(ongoingWarrant);
                await _database.SaveChangesAsync();
                
                _trApiService.AddRequest<TradeRepublicProductPriceResponseDto>(requestString, value =>
                {
                    Thread.Sleep(1000 * 10);
                    
                    // throw new Exception("Test");
                    
                    return OngoingTradeResponse.Complete;
                });

                return _mapper.Map<OngoingWarrantPositionResponseDto>(ongoingWarrant);
            }
            catch (Exception e)
            {
                return new ErrorResponse(new UnexpectedError
                {
                    Title = "Could not save ongoing warrant",
                    Message = "Failed to save or process ongoing warrant trade",
                    Exception = e,
                    AdditionalData = new { Dto = dto, OngoingWarrant = ongoingWarrant },
                }, HttpStatusCode.InternalServerError);
            }
        }

        // public async Task<OneOf<OngoingWarrantPositionResponseDto, ErrorResponse>> CloseOngoingWarrant(OngoingPositionRequestDto dto, string uid)
        // {
        //     if ((await IsStockMarketOpen(dto.Isin)).TryPickT1(out var errorResponse1, out _))
        //         return errorResponse1;
        //     
        //     
        // }
        
        private async Task<OneOf<Success, ErrorResponse>> IsStockMarketOpen(string isin)
        {
            if ((await _trApiService.IsStockMarketOpen(isin)).TryPickT1(out var unexpectedError, out var isStockMarketOpen))
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
        
        private async Task<OneOf<T, ErrorResponse>> MakeTrRequest<T>(string requestString, OngoingPositionRequestDto dto)
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
                    Message = "Fetching product using trade republic api took too long.",
                    AdditionalData = new {dto}
                }, HttpStatusCode.InternalServerError);
            }
            finally { cts.Dispose(); }

            return trResponse;
        }
    }
}