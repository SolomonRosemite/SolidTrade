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
using SolidTradeServer.Data.Dtos.OngoingKnockout.Response;
using SolidTradeServer.Data.Dtos.Shared.OngoingPosition.Request;
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
    public class OngoingKnockoutService
    {
        private readonly TradeRepublicApiService _trApiService;
        private readonly DbSolidTrade _database;
        private readonly IMapper _mapper;

        public OngoingKnockoutService(DbSolidTrade database, IMapper mapper, TradeRepublicApiService trApiService)
        {
            _trApiService = trApiService;
            _database = database;
            _mapper = mapper;
        }

        public async Task<OneOf<OngoingKnockoutPositionResponseDto, ErrorResponse>> GetOngoingKnockout(int id, string uid)
        {
            var user = await _database.Users.AsQueryable()
                .FirstOrDefaultAsync(u => u.Portfolio.OngoingKnockOutPositions.Any(ow => ow.Id == id));

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

            var knockout = await _database.OngoingKnockoutPositions.FindAsync(id);

            if (knockout is null)
            {
                return new ErrorResponse(new NotFound
                {
                    Title = "Ongoing knockout not found",
                    Message = $"Ongoing knockout with id: {id} could not be found",
                }, HttpStatusCode.NotFound);
            }

            return _mapper.Map<OngoingKnockoutPositionResponseDto>(knockout);
        }
        
        public async Task<OneOf<OngoingKnockoutPositionResponseDto, ErrorResponse>> OpenOngoingKnockout(OngoingPositionRequestDto dto, string uid)
        {
            var result = await _trApiService.ValidateRequest(dto.Isin);

            if (result.TryPickT1(out var errorResponse, out _))
                return errorResponse;
            
            var isinWithoutExchangeExtension = ToIsinWithoutExchangeExtension(dto.Isin);

            
            if ((await _trApiService.MakeTrRequest<TradeRepublicProductPriceResponseDto>(GetTradeRepublicProductPriceRequestString(dto.Isin))).TryPickT1(
                out errorResponse, out var trResponse))
                return errorResponse;

            var isFulfilled = GetOngoingProductHandler(dto.Type!.Value, trResponse, dto.PriceThreshold);

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

            var existingKnockout = await _database.KnockoutPositions.AsQueryable()
                .FirstOrDefaultAsync(w => w.Isin == isinWithoutExchangeExtension && w.Portfolio.Id == user.Id);
            
            var ongoingKnockout = new OngoingKnockoutPosition
            {
                Isin = isinWithoutExchangeExtension,
                Portfolio = user.Portfolio,
                Type = dto.Type!.Value,
                GoodUntil = dto.GoodUntil!.Value,                                                                                                                                                                  
                CurrentKnockoutPosition = existingKnockout,
                NumberOfShares = dto.NumberOfShares,
                Price = dto.PriceThreshold,
            };

            try
            {
                var entity = _database.OngoingKnockoutPositions.Add(ongoingKnockout);
                await _database.SaveChangesAsync();
                
                _trApiService.AddOngoingRequestRequest(dto.Isin, PositionType.Knockout, entity.Entity.Id);

                return _mapper.Map<OngoingKnockoutPositionResponseDto>(ongoingKnockout);
            }
            catch (Exception e)
            {
                return new ErrorResponse(new UnexpectedError
                {
                    Title = "Could not save ongoing knockout",
                    Message = "Failed to save or process ongoing knockout trade",
                    Exception = e,
                    AdditionalData = new { Dto = dto, OngoingKnockout = ongoingKnockout },
                }, HttpStatusCode.InternalServerError);
            }
        }

        public async Task<OneOf<OngoingKnockoutPositionResponseDto, ErrorResponse>> CloseOngoingKnockout(CloseOngoingPositionRequestDto dto, string uid)
        {
            var knockout = await _database.OngoingKnockoutPositions.AsQueryable()
                .FirstOrDefaultAsync(w => w.Id == dto.Id && w.Portfolio.User.Uid == uid);

            if (knockout is null)
            {
                return new ErrorResponse(new NotFound
                {
                    Title = "Unable to delete ongoing trade",
                    Message = $"The knockout with id: {dto.Id} could not be found or the user does not own this ongoing knockout.",
                    UserFriendlyMessage = "Could not remove knockout. The knockout might already been filled.",
                    AdditionalData = new { dto, uid }
                }, HttpStatusCode.BadRequest);
            }
            
            try
            {
                _database.OngoingKnockoutPositions.Remove(knockout);
                
                await _database.SaveChangesAsync();
                return _mapper.Map<OngoingKnockoutPositionResponseDto>(knockout);
            }
            catch (Exception e)
            {
                return new ErrorResponse(new UnexpectedError
                {
                    Title = "Could remove ongoing knockout",
                    Message = "Failed to close position.",
                    Exception = e,
                    UserFriendlyMessage = "Something went very wrong. Please try again later.",
                    AdditionalData = new { dto, uid, Knockout = knockout },
                }, HttpStatusCode.InternalServerError);
            }
        }
    }
}