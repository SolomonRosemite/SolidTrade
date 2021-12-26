using System;
using System.Linq;
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

        // Updates create at and updated at automatically
        public override int SaveChanges()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntity && e.State is EntityState.Added or EntityState.Modified);

            foreach (var entityEntry in entries)
            {
                ((BaseEntity)entityEntry.Entity).UpdatedAt = DateTime.Now;

                if (entityEntry.State == EntityState.Added)
                {
                    ((BaseEntity)entityEntry.Entity).CreatedAt = DateTime.Now;
                }
            }

            return base.SaveChanges();
        }
    }
}