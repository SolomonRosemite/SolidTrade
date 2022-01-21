using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OneOf;
using Serilog;
using SolidTradeServer.Data.Common;
using SolidTradeServer.Data.Dtos.HistoricalPosition.Response;
using SolidTradeServer.Data.Entities;
using SolidTradeServer.Data.Models.Errors;
using SolidTradeServer.Data.Models.Errors.Common;

namespace SolidTradeServer.Services
{
    public class HistoricalPositionsService
    {
        private readonly ILogger _logger = Log.ForContext<HistoricalPositionsService>();
        private readonly DbSolidTrade _database;
        private readonly IMapper _mapper;

        public HistoricalPositionsService(DbSolidTrade database, IMapper mapper)
        {
            _database = database;
            _mapper = mapper;
        }

        public async Task<OneOf<IEnumerable<HistoricalPositionResponseDto>, ErrorResponse>> GetHistoricalPositions(int userId, string uid)
        {
            var user = await _database.Users.FindAsync(userId);

            if (user is null)
            {
                return new ErrorResponse(new NotFound
                {
                    Title = "User not found",
                    Message = $"Could not find user with id: {userId}",
                }, HttpStatusCode.NotFound);
            }
            
            if (!user.HasPublicPortfolio && user.Uid != uid)
            {
                return new ErrorResponse(new NotAuthorized
                {
                    Title = "Portfolio is private",
                    Message = "Tried to access other user's portfolio",
                }, HttpStatusCode.Unauthorized);
            }

            var historicalPositions = await _database.HistoricalPositions
                .AsQueryable()
                .Where(p => p.UserId == userId)
                .ToListAsync();
            
            _logger.Information("User with user uid {@Uid} fetched portfolio with user id {@UserId} successfully", uid, userId);

            return _mapper.Map<List<HistoricalPosition>, List<HistoricalPositionResponseDto>>(historicalPositions);
        }
    }
}