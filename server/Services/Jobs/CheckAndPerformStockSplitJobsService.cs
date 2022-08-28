using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SolidTradeServer.Common;
using SolidTradeServer.Data.Common;
using SolidTradeServer.Data.Dtos.TradeRepublic;
using SolidTradeServer.Data.Models.Common.Position;
using SolidTradeServer.Data.Models.Enums;
using SolidTradeServer.Data.Models.Errors;
using SolidTradeServer.Services.TradeRepublic;

namespace SolidTradeServer.Services.Jobs
{
    public class CheckAndPerformStockSplitJobsService
    {
        private readonly ILogger _logger = Log.ForContext<CheckAndPerformStockSplitJobsService>();
        private readonly IServiceScopeFactory _scopeFactory;
        
        public CheckAndPerformStockSplitJobsService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        
        public async Task StartAsync()
        {
            try
            {
                _logger.Information("Checking for stock splits where needed");

                var (historicalPositions, stockPositions) = await GetStockPositions();

                var unperformedHistoricalPositionsStockSplits = await CheckForStockSplits(historicalPositions);
                var unperformedStockSplits = await CheckForStockSplits(stockPositions);
                
                _logger.Information(
                    "Found {@NumberOfUnperformedStockSplits} unperformed historical position stock splits from database",
                    unperformedHistoricalPositionsStockSplits.Count);
                
                _logger.Information(
                    "Found {@NumberOfUnperformedStockSplits} unperformed normal stock position stock splits from database",
                    unperformedStockSplits.Count);

                await PerformStockSplits(unperformedHistoricalPositionsStockSplits);
                _logger.Information(
                    "Successfully performed {@NumberOfPerformedStockSplits} stock splits of type @{StockClassType}",
                    unperformedHistoricalPositionsStockSplits.Count, "HistoricalPositionsStock");
                
                await PerformStockSplits(unperformedStockSplits);
                _logger.Information(
                    "Successfully performed {@NumberOfPerformedStockSplits} stock splits of type @{StockClassType}",
                    unperformedStockSplits.Count, "StockPosition");
            }
            catch (Exception e)
            {
                _logger.Error(Shared.LogMessageTemplate, new UnexpectedError
                {
                    Title = "Could not check for or perform stock split",
                    Message = "Something went wrong trying to check or perform stock splits. See exception for more.",
                    Exception = e,
                });
            }
        }

        private async Task<List<(IPosition, TradeRepublicProductInfoDto)>> CheckForStockSplits(List<IPosition> positions)
        {
            var trApi = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<TradeRepublicApiService>();

            var unperformedStockSplits = new List<(IPosition, TradeRepublicProductInfoDto)>();
            var cachedTradeRepublicProductInfoDtos = new List<TradeRepublicProductInfoDto>();
            foreach (var stock in positions)
            {
                TradeRepublicProductInfoDto stockInfo = cachedTradeRepublicProductInfoDtos.FirstOrDefault(d => d.Isin == stock.Isin);
                
                if (stockInfo is null)
                {
                    var dto = await trApi.MakeTrRequest<TradeRepublicProductInfoDto>(Shared.GetTradeRepublicProductInfoRequestString(stock.Isin));

                    if (dto.TryPickT1(out var err, out stockInfo))
                    {
                        _logger.Warning(Shared.LogMessageTemplate, err);
                        continue;
                    }
                    
                    cachedTradeRepublicProductInfoDtos.Add(stockInfo);
                }

                var hadStockSplit = stockInfo.Splits.Any(s => s.Date > stock.UpdatedAt.ToUnixTimeMilliseconds());

                if (hadStockSplit)
                    unperformedStockSplits.Add((stock, stockInfo));
            }
            
            return unperformedStockSplits;
        }

        private async Task<(List<IPosition>, List<IPosition>)> GetStockPositions()
        {
            await using DbSolidTrade database = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<DbSolidTrade>();

            var historicalPositions = await database.HistoricalPositions
                .AsQueryable()
                .Where(p => p.PositionType == PositionType.Stock)
                .ToListAsync();

            var stockPositions = await database.StockPositions.AsQueryable().ToListAsync();

            return (new List<IPosition>(historicalPositions), new List<IPosition>(stockPositions));
        }

        private async Task PerformStockSplits(IReadOnlyList<(IPosition, TradeRepublicProductInfoDto)> unperformedStockSplits)
        {
            await using DbSolidTrade database = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<DbSolidTrade>();

            for (int i = 0; i < unperformedStockSplits.Count; i++)
            {
                var (position, infoDto) = unperformedStockSplits[i];
                
                foreach (var stockSplitInfo in infoDto.Splits)
                    if (stockSplitInfo.Date > position.UpdatedAt.ToUnixTimeMilliseconds())
                        position = PerformStockSplit(position, stockSplitInfo);
            }
            
            database.UpdateRange(unperformedStockSplits.Select(s => s.Item1));
            await database.SaveChangesAsync();
        }

        private IPosition PerformStockSplit(IPosition position, StockSplitInfo infoDto)
        {
            _logger.Information(
                "Performing a {@Initial} to {@Final} stock split with id {@StockPositionId} and isin {@StockPositionIsin}",
                infoDto.Initial, infoDto.Final, position.Id, position.Isin);

            position.NumberOfShares *= infoDto.Initial * infoDto.Final;
            return position;
        }
    }
}