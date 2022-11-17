﻿using Application.Common;
using Application.Common.Interfaces.Persistence;
using Application.Common.Interfaces.Persistence.Database;
using Application.Common.Interfaces.Services.Jobs;
using Application.Common.Interfaces.Services.TradeRepublic;
using Application.Errors.Common;
using Application.Models.Dtos.TradeRepublic;
using Domain.Common.Position;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Application.Services.Jobs
{
    public class CheckAndPerformStockSplitJob : IBackgroundJob<CheckAndPerformStockSplitJob>
    {
        public string JobTitle => "Check and perform stock splits";
        
        private readonly ILogger _logger = Log.ForContext<CheckAndPerformStockSplitJob>();
        private readonly IServiceScopeFactory _scopeFactory;
        
        public CheckAndPerformStockSplitJob(IServiceScopeFactory scopeFactory)
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
                _logger.Error(ApplicationConstants.LogMessageTemplate, new UnexpectedError
                {
                    Title = "Could not check for or perform stock split",
                    Message = "Something went wrong trying to check or perform stock splits. See exception for more.",
                    Exception = e,
                });
            }
        }

        private async Task<List<(IPosition, TradeRepublicProductInfoDto)>> CheckForStockSplits(List<IPosition> positions)
        {
            var trApi = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ITradeRepublicApiService>();

            var unperformedStockSplits = new List<(IPosition, TradeRepublicProductInfoDto)>();
            var cachedTradeRepublicProductInfoDtos = new List<TradeRepublicProductInfoDto>();
            foreach (var stock in positions)
            {
                var stockInfo = cachedTradeRepublicProductInfoDtos.FirstOrDefault(d => d.Isin == stock.Isin);
                
                if (stockInfo is null)
                {
                    var dto = await trApi.MakeTrRequest<TradeRepublicProductInfoDto>(Shared.GetTradeRepublicProductInfoRequestString(stock.Isin));

                    if (dto.TryPickT1(out var err, out stockInfo))
                    {
                        _logger.Warning(ApplicationConstants.LogMessageTemplate, err);
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
            await using IApplicationDbContext database = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<IApplicationDbContext>();

            var historicalPositions = await database.HistoricalPositions
                .AsQueryable()
                .Where(p => p.PositionType == PositionType.Stock)
                .ToListAsync();

            var stockPositions = await database.StockPositions.AsQueryable().ToListAsync();

            return (new List<IPosition>(historicalPositions), new List<IPosition>(stockPositions));
        }

        private async Task PerformStockSplits(IReadOnlyList<(IPosition, TradeRepublicProductInfoDto)> unperformedStockSplits)
        {
            await using IApplicationDbContext database = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<IApplicationDbContext>();

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