using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SolidTradeServer.Common;
using SolidTradeServer.Data.Common;
using SolidTradeServer.Data.Models.Errors;

namespace SolidTradeServer.Services.Jobs
{
    public class RemoveOngoingExpiredTradeJobsService
    {
        private readonly ILogger _logger = Log.ForContext<RemoveOngoingExpiredTradeJobsService>();
        private readonly IServiceScopeFactory _scopeFactory;
        
        public RemoveOngoingExpiredTradeJobsService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        
        public Task StartAsync()
        {
            try
            {
                _logger.Information("Removing expired ongoing trades from database.");
                return RemoveExpiredTradesAsync();
            }
            catch (Exception e)
            {
                _logger.Error(Shared.LogMessageTemplate, new UnexpectedError
                {
                    Title = "Could not remove expired trades",
                    Message = "Something went wrong trying to remove expired trades. See exception for more.",
                    Exception = e,
                });
                return Task.CompletedTask;
            }
        }

        private async Task RemoveExpiredTradesAsync()
        {
            await using DbSolidTrade database = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<DbSolidTrade>();

            var expiredKnockouts = database.OngoingKnockoutPositions.AsQueryable().Where(p => DateTimeOffset.Now > p.GoodUntil);
            var expiredWarrants = database.OngoingWarrantPositions.AsQueryable().Where(p => DateTimeOffset.Now > p.GoodUntil);

            var expiredKnockoutsCount = await expiredKnockouts.CountAsync();
            var expiredWarrantsCount = await expiredWarrants.CountAsync();
            
            database.OngoingKnockoutPositions.RemoveRange(expiredKnockouts);
            database.OngoingWarrantPositions.RemoveRange(expiredWarrants);

            await database.SaveChangesAsync();
            _logger.Information("Removed {@expiredKnockoutsCount} expired ongoing knockouts and {@expiredWarrantsCount} expired ongoing warrants from database.", expiredKnockoutsCount, expiredWarrantsCount);
        }
    }
}