using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OneOf;
using Serilog;
using SolidTradeServer.Common;
using SolidTradeServer.Data.Common;
using SolidTradeServer.Data.Dtos.Shared.Common;
using SolidTradeServer.Data.Dtos.TradeRepublic;
using SolidTradeServer.Data.Dtos.Warrant.Response;
using SolidTradeServer.Data.Models.Errors;
using SolidTradeServer.Data.Models.Errors.Common;
using SolidTradeServer.Services.TradeRepublic;

namespace SolidTradeServer.Services.Jobs
{
    public class RemoveExpiredWarrantProductsJobsService
    {
        private readonly ILogger _logger = Log.ForContext<RemoveExpiredWarrantProductsJobsService>();
        private readonly TradeRepublicApiService _tradeRepublicApiService;
        private readonly IServiceScopeFactory _scopeFactory;
        
        public RemoveExpiredWarrantProductsJobsService(TradeRepublicApiService tradeRepublicApiService, IServiceScopeFactory scopeFactory)
        {
            _tradeRepublicApiService = tradeRepublicApiService;
            _scopeFactory = scopeFactory;
        }
        
        public Task StartAsync()
        {
            try
            {
                _logger.Information("Removing expired warrant products from database.");
                return RemoveKnockedOutProducts();
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

        private async Task RemoveKnockedOutProducts()
        {
            await using DbSolidTrade database = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<DbSolidTrade>();
            using WarrantService warrantService = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<WarrantService>();
            
            var warrantPositions = await database.WarrantPositions
                .Include(w => w.Portfolio).ThenInclude(p => p.User).AsQueryable().ToListAsync();
            
            var tasks = new List<Task<OneOf<TradeRepublicProductInfoDto, ErrorResponse>>>();
            
            foreach (var warrantPosition in warrantPositions)
            {
                tasks.Add(_tradeRepublicApiService.MakeTrRequest<TradeRepublicProductInfoDto>(
                    Shared.GetTradeRepublicProductInfoRequestString(warrantPosition.Isin)));
            }
            
            var results = await Task.WhenAll(tasks);

            var trProductInfos = results.Where(oneOfResult => oneOfResult.IsT0).Select(oneOfResult => oneOfResult.AsT0);
            var errors = results.Where(oneOfResult => oneOfResult.IsT1).Select(oneOfResult => oneOfResult.AsT1);

            if (errors.Any())
            {
                foreach (var error in errors)
                    _logger.Warning(Shared.LogMessageTemplate, error);
                return;
            }

            var warrantsTasks = new List<Task<OneOf<WarrantPositionResponseDto, ErrorResponse>>>();
            foreach (var trProductInfo in trProductInfos)
            {
                var warrantPosition = warrantPositions.Find(k => k.Isin == trProductInfo.Isin);
                
                // We could also check if it is still active or not. The result would be the same.
                if (DateTime.Now > trProductInfo.DerivativeInfo.Properties.Expiry)
                {
                    var warrantTask = warrantService.SellWarrantInternal(database,
                        new BuyOrSellRequestDto
                            {Isin = $"{warrantPosition!.Isin}.{trProductInfo.ExchangeIds.First()}", NumberOfShares = warrantPosition.NumberOfShares},
                        warrantPosition.Portfolio.User);
                    
                    warrantsTasks.Add(warrantTask);
                }
            }
            
            var warrantResults = await Task.WhenAll(warrantsTasks);

            var warrantPositionDtos = warrantResults.Where(oneOfResult => oneOfResult.IsT0).Select(oneOfResult => oneOfResult.AsT0);
            var warrantErrors = warrantResults.Where(oneOfResult => oneOfResult.IsT1).Select(oneOfResult => oneOfResult.AsT1);

            foreach (var errorResponse in warrantErrors)
                _logger.Warning(Shared.LogMessageTemplate, errorResponse.Error);
            
            _logger.Information("Removed {@RemovedWarrantPositionCount} expired warrants from database.", warrantPositionDtos.Count());
        }
    }
}