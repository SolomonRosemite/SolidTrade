using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using OneOf;
using SolidTradeServer.Data.Common;
using SolidTradeServer.Data.Dtos.Warrant.Request;
using SolidTradeServer.Data.Dtos.Warrant.Response;
using SolidTradeServer.Data.Dtos.Warrant.TradeRepublic;
using SolidTradeServer.Data.Models.Errors;
using SolidTradeServer.Data.Models.Errors.Common;
using SolidTradeServer.Services.Common;

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
            var requestString = "{\"type\":\"ticker\",\"id\":\"" + dto.WarrantIsin + "\"}";

            var cts = new CancellationTokenSource();
            string result;

            try
            {
                cts.CancelAfter(1000 * 10);
                result = await _trApiService.AddRequest(requestString, cts.Token);
            }
            catch (OperationCanceledException)
            {
                cts.Dispose();
                return new ErrorResponse(new UnexpectedError
                {
                    Title = "Task timeout",
                    Message = "Fetching warrant using trade republic api took too long.",
                    AdditionalData = new { dto }
                }, HttpStatusCode.InternalServerError);
            }
            finally { cts.Dispose(); }

            TradeRepublicWarrantResponseDto trResponse;
            
            try
            {
                trResponse = JsonSerializer.Deserialize<TradeRepublicWarrantResponseDto>(result, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception e)
            {
                return new ErrorResponse(new UnexpectedError
                {
                    Title = "Unable to deserialize json",
                    Message = "Unable to deserialize trade republic json response.",
                    Exception = e,
                    AdditionalData = new { dto, result },
                }, HttpStatusCode.InternalServerError);
            }

            var totalPrice = trResponse!.Ask.Price * dto.NumberOfShares;

            // Todo: Check if the user balance is sufficient.
            // Also for the ongoing order. When to order get a fill check first if the balance is sufficient.
            Console.WriteLine(totalPrice);
            
            return new OneOf<WarrantPositionResponseDto, ErrorResponse>();
        }

        // public async Task<OneOf<WarrantPositionResponseDto, ErrorResponse>> SellWarrant(BuyWarrantRequestDto dto, string uid)
        // {
        //     var user = await _database.Users.FirstOrDefaultAsync(u => u.Uid == uid);
        //     
        //     
        // }
    }
}