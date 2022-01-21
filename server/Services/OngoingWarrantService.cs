using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OneOf;
using Serilog;
using SolidTradeServer.Data.Common;
using SolidTradeServer.Data.Dtos.OngoingWarrant.Response;
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
    public class OngoingWarrantService
    {
        private readonly ILogger _logger = Log.ForContext<OngoingWarrantService>();
        
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
            var user = await _database.Users.AsQueryable()
                .FirstOrDefaultAsync(u => u.Portfolio.OngoingWarrantPositions.Any(ow => ow.Id == id));

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
            
            _logger.Information("User with user uid {@Uid} fetched ongoing warrant with ongoing warrant id {@OngoingWarrantId} successfully", uid, id);

            return _mapper.Map<OngoingWarrantPositionResponseDto>(warrant);
        }
        
        public async Task<OneOf<OngoingWarrantPositionResponseDto, ErrorResponse>> OpenOngoingWarrant(OngoingPositionRequestDto dto, string uid)
        {
            var result = await _trApiService.ValidateRequest(dto.Isin);

            if (result.TryPickT1(out var errorResponse, out _))
                return errorResponse;

            if ((await _trApiService.MakeTrRequest<TradeRepublicProductPriceResponseDto>(GetTradeRepublicProductPriceRequestString(dto.Isin))).TryPickT1(
                out errorResponse, out var trResponse))
                return errorResponse;

            var isinWithoutExchangeExtension = ToIsinWithoutExchangeExtension(dto.Isin);
            
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

            var existingWarrant = await _database.WarrantPositions.AsQueryable()
                .FirstOrDefaultAsync(w => w.Isin == isinWithoutExchangeExtension && w.Portfolio.Id == user.Id);
            
            var ongoingWarrant = new OngoingWarrantPosition
            {
                Isin = isinWithoutExchangeExtension,
                Portfolio = user.Portfolio,
                Type = dto.Type!.Value,
                GoodUntil = dto.GoodUntil!.Value,                                                                                                                                                                  
                CurrentWarrantPosition = existingWarrant,
                NumberOfShares = dto.NumberOfShares,
                Price = dto.PriceThreshold,
            };

            try
            {
                var entity = _database.OngoingWarrantPositions.Add(ongoingWarrant);
                
                _logger.Information("Trying to save open ongoing warrant with isin {@Isin} for User with uid {@Uid}", dto.Isin, uid);
                await _database.SaveChangesAsync();
                _logger.Information("Save open ongoing warrant with isin {@Isin} for User with uid {@Uid} was successful", dto.Isin, uid);
                
                _logger.Information("Add ongoing warrant with isin {@Isin} to trade republic ongoing requests", dto.Isin);
                
                _trApiService.AddOngoingRequest(dto.Isin, PositionType.Warrant, entity.Entity.Id);

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

        public async Task<OneOf<OngoingWarrantPositionResponseDto, ErrorResponse>> CloseOngoingWarrant(CloseOngoingPositionRequestDto dto, string uid)
        {
            var warrant = await _database.OngoingWarrantPositions.AsQueryable()
                .FirstOrDefaultAsync(w => w.Id == dto.Id && w.Portfolio.User.Uid == uid);

            if (warrant is null)
            {
                return new ErrorResponse(new NotFound
                {
                    Title = "Unable to delete ongoing trade",
                    Message = $"The warrant with id: {dto.Id} could not be found or the user does not own this ongoing warrant.",
                    UserFriendlyMessage = "Could not remove warrant. The warrant might already been filled.",
                    AdditionalData = new { dto, uid }
                }, HttpStatusCode.BadRequest);
            }
            
            try
            {
                _database.OngoingWarrantPositions.Remove(warrant);
                
                _logger.Information("Trying to save close ongoing warrant with id {@OngoingWarrantId} for User with uid {@Uid}", dto.Id, uid);
                await _database.SaveChangesAsync();
                _logger.Information("Save close ongoing warrant with id {@OngoingWarrantId} for User with uid {@Uid} was successful", dto.Id, uid);
                
                return _mapper.Map<OngoingWarrantPositionResponseDto>(warrant);
            }
            catch (Exception e)
            {
                return new ErrorResponse(new UnexpectedError
                {
                    Title = "Could remove ongoing warrant",
                    Message = "Failed to close position.",
                    Exception = e,
                    UserFriendlyMessage = "Something went very wrong. Please try again later.",
                    AdditionalData = new { dto, uid, warrant },
                }, HttpStatusCode.InternalServerError);
            }
        }
    }
}