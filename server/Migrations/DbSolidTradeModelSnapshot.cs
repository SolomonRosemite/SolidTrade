﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SolidTradeServer.Data.Common;

namespace SolidTradeServer.Migrations
{
    [DbContext(typeof(DbSolidTrade))]
    partial class DbSolidTradeModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
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

                    b.Property<float>("BuyInPrice")
                        .HasColumnType("real");

                    b.Property<int>("BuyOrSell")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Isin")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("NumberOfShares")
                        .HasColumnType("int");

                    b.Property<float>("Performance")
                        .HasColumnType("real");

                    b.Property<int>("PositionType")
                        .HasColumnType("int");

                    b.Property<byte[]>("TimeStamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.ToTable("HistoricalPositions");
                });

            modelBuilder.Entity("SolidTradeServer.Data.Entities.KnockoutPosition", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<float>("BuyInPrice")
                        .HasColumnType("real");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Isin")
                        .HasMaxLength(12)
                        .HasColumnType("char(12)");

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

                    b.Property<string>("Isin")
                        .HasMaxLength(12)
                        .HasColumnType("char(12)");

                    b.Property<int?>("PortfolioId")
                        .HasColumnType("int");

                    b.Property<float>("Price")
                        .HasColumnType("real");

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

                    b.Property<string>("Isin")
                        .HasMaxLength(12)
                        .HasColumnType("char(12)");

                    b.Property<int?>("PortfolioId")
                        .HasColumnType("int");

                    b.Property<float>("Price")
                        .HasColumnType("real");

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

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset");

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

            modelBuilder.Entity("SolidTradeServer.Data.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("DisplayName")
                        .HasMaxLength(32)
                        .HasColumnType("char(32)");

                    b.Property<string>("Email")
                        .HasMaxLength(64)
                        .HasColumnType("char(64)");

                    b.Property<bool>("HasPublicPortfolio")
                        .HasColumnType("bit");

                    b.Property<int?>("HistoricalPositionId")
                        .HasColumnType("int");

                    b.Property<string>("ProfilePictureUrl")
                        .HasMaxLength(255)
                        .HasColumnType("char(255)");

                    b.Property<byte[]>("TimeStamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<DateTimeOffset>("UpdatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Username")
                        .HasMaxLength(32)
                        .HasColumnType("char(32)");

                    b.HasKey("Id");

                    b.HasIndex("HistoricalPositionId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("SolidTradeServer.Data.Entities.WarrantPosition", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<float>("BuyInPrice")
                        .HasColumnType("real");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Isin")
                        .HasMaxLength(12)
                        .HasColumnType("char(12)");

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

                    b.ToTable("WarrantPositions");
                });

            modelBuilder.Entity("SolidTradeServer.Data.Entities.KnockoutPosition", b =>
                {
                    b.HasOne("SolidTradeServer.Data.Entities.Portfolio", "Portfolio")
                        .WithMany("KnockOutPositions")
                        .HasForeignKey("PortfolioId");

                    b.Navigation("Portfolio");
                });

            modelBuilder.Entity("SolidTradeServer.Data.Entities.OngoingKnockoutPosition", b =>
                {
                    b.HasOne("SolidTradeServer.Data.Entities.KnockoutPosition", "CurrentKnockoutPosition")
                        .WithMany()
                        .HasForeignKey("CurrentKnockoutPositionId");

                    b.HasOne("SolidTradeServer.Data.Entities.Portfolio", "Portfolio")
                        .WithMany("OngoingKnockOutPositions")
                        .HasForeignKey("PortfolioId");

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
                        .HasForeignKey("PortfolioId");

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

            modelBuilder.Entity("SolidTradeServer.Data.Entities.User", b =>
                {
                    b.HasOne("SolidTradeServer.Data.Entities.HistoricalPosition", "HistoricalPosition")
                        .WithMany()
                        .HasForeignKey("HistoricalPositionId");

                    b.Navigation("HistoricalPosition");
                });

            modelBuilder.Entity("SolidTradeServer.Data.Entities.WarrantPosition", b =>
                {
                    b.HasOne("SolidTradeServer.Data.Entities.Portfolio", "Portfolio")
                        .WithMany("WarrantPositions")
                        .HasForeignKey("PortfolioId");

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
                    b.Navigation("Portfolio");
                });
#pragma warning restore 612, 618
        }
    }
}
