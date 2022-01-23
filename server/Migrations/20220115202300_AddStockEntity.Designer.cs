﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SolidTradeServer.Data.Common;

namespace SolidTradeServer.Migrations
{
    [DbContext(typeof(DbSolidTrade))]
    [Migration("20220115202300_AddStockEntity")]
    partial class AddStockEntity
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.12")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("SolidTradeServer.Data.Entities.HistoricalPosition", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<decimal>("BuyInPrice")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("BuyOrSell")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Isin")
                        .IsRequired()
                        .HasMaxLength(12)
                        .HasColumnType("nvarchar(12)");

                    b.Property<int>("NumberOfShares")
                        .HasColumnType("int");

                    b.Property<decimal>("Performance")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("PositionType")
                        .HasColumnType("int");

                    b.Property<byte[]>("TimeStamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("HistoricalPositions");
                });

            modelBuilder.Entity("SolidTradeServer.Data.Entities.KnockoutPosition", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<decimal>("BuyInPrice")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Isin")
                        .IsRequired()
                        .HasMaxLength(12)
                        .HasColumnType("nvarchar(12)");

                    b.Property<int>("NumberOfShares")
                        .HasColumnType("int");

                    b.Property<int>("PortfolioId")
                        .HasColumnType("int");

                    b.Property<byte[]>("TimeStamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.HasIndex("PortfolioId");

                    b.ToTable("KnockoutPositions");
                });

            modelBuilder.Entity("SolidTradeServer.Data.Entities.OngoingKnockoutPosition", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<int?>("CurrentKnockoutPositionId")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("GoodUntil")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Isin")
                        .IsRequired()
                        .HasMaxLength(12)
                        .HasColumnType("nvarchar(12)");

                    b.Property<int>("NumberOfShares")
                        .HasColumnType("int");

                    b.Property<int>("PortfolioId")
                        .HasColumnType("int");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.Property<byte[]>("TimeStamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.HasIndex("CurrentKnockoutPositionId");

                    b.HasIndex("PortfolioId");

                    b.ToTable("OngoingKnockoutPositions");
                });

            modelBuilder.Entity("SolidTradeServer.Data.Entities.OngoingWarrantPosition", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<int?>("CurrentWarrantPositionId")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("GoodUntil")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Isin")
                        .IsRequired()
                        .HasMaxLength(12)
                        .HasColumnType("nvarchar(12)");

                    b.Property<int>("NumberOfShares")
                        .HasColumnType("int");

                    b.Property<int>("PortfolioId")
                        .HasColumnType("int");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.Property<byte[]>("TimeStamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.HasIndex("CurrentWarrantPositionId");

                    b.HasIndex("PortfolioId");

                    b.ToTable("OngoingWarrantPositions");
                });

            modelBuilder.Entity("SolidTradeServer.Data.Entities.Portfolio", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<decimal>("Balance")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<decimal>("InitialBalance")
                        .HasColumnType("decimal(18,2)");

                    b.Property<byte[]>("TimeStamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("Portfolios");
                });

            modelBuilder.Entity("SolidTradeServer.Data.Entities.StockPosition", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<decimal>("BuyInPrice")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Isin")
                        .IsRequired()
                        .HasMaxLength(12)
                        .HasColumnType("nvarchar(12)");

                    b.Property<int>("NumberOfShares")
                        .HasColumnType("int");

                    b.Property<int?>("PortfolioId")
                        .HasColumnType("int");

                    b.Property<byte[]>("TimeStamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.HasIndex("PortfolioId");

                    b.ToTable("StockPositions");
                });

            modelBuilder.Entity("SolidTradeServer.Data.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<bool>("HasPublicPortfolio")
                        .HasColumnType("bit");

                    b.Property<string>("ProfilePictureUrl")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<byte[]>("TimeStamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<string>("Uid")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("SolidTradeServer.Data.Entities.WarrantPosition", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<decimal>("BuyInPrice")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Isin")
                        .IsRequired()
                        .HasMaxLength(12)
                        .HasColumnType("nvarchar(12)");

                    b.Property<int>("NumberOfShares")
                        .HasColumnType("int");

                    b.Property<int>("PortfolioId")
                        .HasColumnType("int");

                    b.Property<byte[]>("TimeStamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.HasIndex("PortfolioId");

                    b.ToTable("WarrantPositions");
                });

            modelBuilder.Entity("SolidTradeServer.Data.Entities.HistoricalPosition", b =>
                {
                    b.HasOne("SolidTradeServer.Data.Entities.User", null)
                        .WithMany("HistoricalPositions")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("SolidTradeServer.Data.Entities.KnockoutPosition", b =>
                {
                    b.HasOne("SolidTradeServer.Data.Entities.Portfolio", "Portfolio")
                        .WithMany("KnockOutPositions")
                        .HasForeignKey("PortfolioId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Portfolio");
                });

            modelBuilder.Entity("SolidTradeServer.Data.Entities.OngoingKnockoutPosition", b =>
                {
                    b.HasOne("SolidTradeServer.Data.Entities.KnockoutPosition", "CurrentKnockoutPosition")
                        .WithMany()
                        .HasForeignKey("CurrentKnockoutPositionId");

                    b.HasOne("SolidTradeServer.Data.Entities.Portfolio", "Portfolio")
                        .WithMany("OngoingKnockOutPositions")
                        .HasForeignKey("PortfolioId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CurrentKnockoutPosition");

                    b.Navigation("Portfolio");
                });

            modelBuilder.Entity("SolidTradeServer.Data.Entities.OngoingWarrantPosition", b =>
                {
                    b.HasOne("SolidTradeServer.Data.Entities.WarrantPosition", "CurrentWarrantPosition")
                        .WithMany()
                        .HasForeignKey("CurrentWarrantPositionId");

                    b.HasOne("SolidTradeServer.Data.Entities.Portfolio", "Portfolio")
                        .WithMany("OngoingWarrantPositions")
                        .HasForeignKey("PortfolioId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CurrentWarrantPosition");

                    b.Navigation("Portfolio");
                });

            modelBuilder.Entity("SolidTradeServer.Data.Entities.Portfolio", b =>
                {
                    b.HasOne("SolidTradeServer.Data.Entities.User", "User")
                        .WithOne("Portfolio")
                        .HasForeignKey("SolidTradeServer.Data.Entities.Portfolio", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("SolidTradeServer.Data.Entities.StockPosition", b =>
                {
                    b.HasOne("SolidTradeServer.Data.Entities.Portfolio", "Portfolio")
                        .WithMany()
                        .HasForeignKey("PortfolioId");

                    b.Navigation("Portfolio");
                });

            modelBuilder.Entity("SolidTradeServer.Data.Entities.WarrantPosition", b =>
                {
                    b.HasOne("SolidTradeServer.Data.Entities.Portfolio", "Portfolio")
                        .WithMany("WarrantPositions")
                        .HasForeignKey("PortfolioId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Portfolio");
                });

            modelBuilder.Entity("SolidTradeServer.Data.Entities.Portfolio", b =>
                {
                    b.Navigation("KnockOutPositions");

                    b.Navigation("OngoingKnockOutPositions");

                    b.Navigation("OngoingWarrantPositions");

                    b.Navigation("WarrantPositions");
                });

            modelBuilder.Entity("SolidTradeServer.Data.Entities.User", b =>
                {
                    b.Navigation("HistoricalPositions");

                    b.Navigation("Portfolio");
                });
#pragma warning restore 612, 618
        }
    }
}
