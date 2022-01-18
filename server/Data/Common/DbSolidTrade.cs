using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SolidTradeServer.Data.Entities;
using SolidTradeServer.Data.Entities.Common;

namespace SolidTradeServer.Data.Common
{
    public class DbSolidTrade : DbContext
    {
        public DbSolidTrade()
        {
        }

        public DbSolidTrade(DbContextOptions<DbContext> options)
            : base(options)
        {
        }
        
        public DbSet<HistoricalPosition> HistoricalPositions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Portfolio> Portfolios { get; set; }
        public DbSet<StockPosition> StockPositions { get; set; }
        public DbSet<WarrantPosition> WarrantPositions { get; set; }
        public DbSet<KnockoutPosition> KnockoutPositions { get; set; }
        public DbSet<OngoingWarrantPosition> OngoingWarrantPositions { get; set; }
        public DbSet<OngoingKnockoutPosition> OngoingKnockoutPositions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.credentials.json")
                .Build();
            
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("SqlServerConnection"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(u => u.Portfolio)
                .WithOne(p => p.User)
                .HasForeignKey<Portfolio>(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.HistoricalPositions)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
            
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetPrecision(18);
                property.SetScale(4);
            }
        }
        
        // Updates create at and updated at automatically
        public override int SaveChanges()
        {
            OnSaveChangesOverride();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnSaveChangesOverride();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            OnSaveChangesOverride();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            OnSaveChangesOverride();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void OnSaveChangesOverride()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntity && e.State is EntityState.Added or EntityState.Modified);

            var dateTimeOffsetNow = DateTimeOffset.Now;
            
            foreach (var entityEntry in entries)
            {
                ((BaseEntity)entityEntry.Entity).UpdatedAt = dateTimeOffsetNow;

                if (entityEntry.State == EntityState.Added)
                {
                    ((BaseEntity)entityEntry.Entity).CreatedAt = dateTimeOffsetNow;
                }
            }
        }
    }
}