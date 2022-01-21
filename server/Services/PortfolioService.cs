using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OneOf;
using Serilog;
using SolidTradeServer.Data.Common;
using SolidTradeServer.Data.Dtos.Portfolio.Request;
using SolidTradeServer.Data.Dtos.Portfolio.Response;
using SolidTradeServer.Data.Models.Errors;
using SolidTradeServer.Data.Models.Errors.Common;

namespace SolidTradeServer.Services
{
    public class PortfolioService
    {
        private readonly ILogger _logger = Log.ForContext<PortfolioService>();
        
        private readonly DbSolidTrade _database;
        private readonly IMapper _mapper;

        public PortfolioService(DbSolidTrade database, IMapper mapper)
        {
            _database = database;
            _mapper = mapper;
        }

        public async Task<OneOf<PortfolioResponseDto, ErrorResponse>> GetPortfolio(GetPortfolioRequestDto dto, string uid)
        {
            var query = _database.Portfolios
                .Include(p => p.User)
                .Include(p => p.WarrantPositions)
                .Include(p => p.StockPositions)
                .Include(p => p.KnockOutPositions);

            if (dto.IncludeOngoingPositions)
            {
                query
                    .Include(p => p.OngoingWarrantPositions)
                    .Include(p => p.OngoingKnockOutPositions);
            }
            
            if (dto.PortfolioId.HasValue)
            {
                var portfolio = await query.FirstOrDefaultAsync(p => p.Id == dto.PortfolioId);

                if (portfolio is null)
                {
                    return new ErrorResponse(new NotFound
                    {
                        Title = "Portfolio not found",
                        Message = $"The portfolio with portfolioId: {dto.PortfolioId} could not be found.",
                    }, HttpStatusCode.NotFound);
                }
                
                if (!portfolio.User.HasPublicPortfolio && portfolio.User.Uid != uid)
                {
                    return new ErrorResponse(new NotAuthorized
                    {
                        Title = "Portfolio is private",
                        Message = "Tried to access other user's portfolio",
                    }, HttpStatusCode.Unauthorized);
                }
                
                _logger.Information("User with user uid {@Uid} fetched portfolio with portfolio id {@PortfolioId} successfully", uid, dto.PortfolioId);

                return _mapper.Map<PortfolioResponseDto>(portfolio);
            }

            var portfolioByUserId = await query.FirstOrDefaultAsync(p => p.UserId == dto.UserId);

            if (portfolioByUserId is null)
            {
                return new ErrorResponse(new NotFound
                {
                    Title = "Portfolio not found",
                    Message = $"The portfolio with userId: {dto.UserId} could not be found.",
                }, HttpStatusCode.NotFound);
            }
            
            if (!portfolioByUserId.User.HasPublicPortfolio && portfolioByUserId.User.Uid != uid)
            {
                return new ErrorResponse(new NotAuthorized
                {
                    Title = "Portfolio is private",
                    Message = "Tried to access other user's portfolio",
                }, HttpStatusCode.Unauthorized);
            }
            
            _logger.Information("User with user uid {@Uid} fetched portfolio by user id {@UserId} successfully", uid, dto.UserId);

            return _mapper.Map<PortfolioResponseDto>(portfolioByUserId);
        }
    }
}