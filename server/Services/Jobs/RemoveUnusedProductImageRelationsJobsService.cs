using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SolidTradeServer.Common;
using SolidTradeServer.Data.Common;
using SolidTradeServer.Data.Dtos.ProductImage.Request;
using SolidTradeServer.Data.Entities;
using SolidTradeServer.Data.Models.Errors;
using SolidTradeServer.Services.Cache;

namespace SolidTradeServer.Services.Jobs
{
    public class RemoveUnusedProductImageRelationsJobsService
    {
        private readonly ILogger _logger = Log.ForContext<RemoveUnusedProductImageRelationsJobsService>();
        private readonly IServiceScopeFactory _scopeFactory;
        
        public RemoveUnusedProductImageRelationsJobsService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        
        public Task StartAsync()
        {
            try
            {
                _logger.Information("Removing unused product image relations from database");
                return RemoveUnusedProductImageRelationsAsync();
            }
            catch (Exception e)
            {
                _logger.Error(Shared.LogMessageTemplate, new UnexpectedError
                {
                    Title = "Could not remove product image relations",
                    Message = "Something went wrong trying to remove expired trades. See exception for more.",
                    Exception = e,
                });
                return Task.CompletedTask;
            }
        }

        private async Task RemoveUnusedProductImageRelationsAsync()
        {
            CloudinaryService cloudinaryService = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<CloudinaryService>();
            await using DbSolidTrade database = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<DbSolidTrade>();
            ICacheService cache = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ICacheService>();

            var productImageRelations = await database.ProductImageRelations.AsQueryable().ToListAsync();

            int countOfSuccessfulDeletions = 0, countOfUnusedProductImageRelations = 0;

            foreach (var productImageRelation in productImageRelations)
            {
                if (await CheckIfImageRelationsIsUnused(database, productImageRelation))
                {
                    countOfUnusedProductImageRelations++;
                    
                    var deletionResult = await cloudinaryService.DeleteImage(productImageRelation.CorrespondingImageUrl);

                    if (deletionResult.TryPickT1(out var error, out _))
                    {
                        _logger.Warning(Shared.LogMessageTemplate, error);
                        continue;
                    }

                    database.ProductImageRelations.Remove(productImageRelation);
                    countOfSuccessfulDeletions++;
                }
            }
            
            // Because the cache service might still have references to images that have been deleted, we remove all cached GetProductImageRequestDto to insure that the cache does not return
            // image urls who's image does not exist anymore.
            // The type GetProductImageRequestDto is the type being cached. This is why we use GetProductImageRequestDto instead of ProductImageRelation.
            cache.Clear<GetProductImageRequestDto>();
            await database.SaveChangesAsync();
            _logger.Information(
                "There are {@CountOfUnusedProductImageRelations} unused product images and {@CountOfSuccessfulDeletions} product images were successfully deleted from database",
                countOfUnusedProductImageRelations, countOfSuccessfulDeletions);
        }

        private async Task<bool> CheckIfImageRelationsIsUnused(DbSolidTrade db, ProductImageRelation relation)
        {
            if (
                await db.KnockoutPositions.AsQueryable().AnyAsync(k => k.Isin == relation.Isin)
                || await db.OngoingKnockoutPositions.AsQueryable().AnyAsync(k => k.Isin == relation.Isin)
                || await db.OngoingWarrantPositions.AsQueryable().AnyAsync(k => k.Isin == relation.Isin)
                || await db.StockPositions.AsQueryable().AnyAsync(k => k.Isin == relation.Isin)
                || await db.WarrantPositions.AsQueryable().AnyAsync(k => k.Isin == relation.Isin))
            {
                return false;
            }

            return true;
        }
    }
}