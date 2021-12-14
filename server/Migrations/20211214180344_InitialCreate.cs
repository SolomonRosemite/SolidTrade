using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SolidTradeServer.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssetInfosSocieteGenerale",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<string>(type: "char(3)", maxLength: 3, nullable: true),
                    Ticker = table.Column<string>(type: "char(12)", maxLength: 12, nullable: true),
                    AssetImageUrl = table.Column<string>(type: "char(255)", maxLength: 255, nullable: true),
                    Name = table.Column<string>(type: "char(128)", maxLength: 128, nullable: true),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetInfosSocieteGenerale", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricalPositions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetInfoId = table.Column<int>(type: "int", nullable: true),
                    PositionType = table.Column<int>(type: "int", nullable: false),
                    LongOrShort = table.Column<int>(type: "int", nullable: false),
                    BuyOrSell = table.Column<int>(type: "int", nullable: false),
                    BuyInPrice = table.Column<float>(type: "real", nullable: false),
                    Performance = table.Column<float>(type: "real", nullable: false),
                    NumberOfShares = table.Column<int>(type: "int", nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricalPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricalPositions_AssetInfosSocieteGenerale_AssetInfoId",
                        column: x => x.AssetInfoId,
                        principalTable: "AssetInfosSocieteGenerale",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KnockoutDerivatives",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetInfoId = table.Column<int>(type: "int", nullable: true),
                    LongOrShort = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "char(6)", maxLength: 6, nullable: true),
                    StrikePrice = table.Column<float>(type: "real", nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnockoutDerivatives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KnockoutDerivatives_AssetInfosSocieteGenerale_AssetInfoId",
                        column: x => x.AssetInfoId,
                        principalTable: "AssetInfosSocieteGenerale",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WarrantDerivatives",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetInfoId = table.Column<int>(type: "int", nullable: true),
                    ExpirationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CallOrPut = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "char(6)", maxLength: 6, nullable: true),
                    StrikePrice = table.Column<float>(type: "real", nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarrantDerivatives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarrantDerivatives_AssetInfosSocieteGenerale_AssetInfoId",
                        column: x => x.AssetInfoId,
                        principalTable: "AssetInfosSocieteGenerale",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HistoricalPositionId = table.Column<int>(type: "int", nullable: true),
                    Username = table.Column<string>(type: "char(32)", maxLength: 32, nullable: true),
                    DisplayName = table.Column<string>(type: "char(32)", maxLength: 32, nullable: true),
                    ProfilePictureUrl = table.Column<string>(type: "char(255)", maxLength: 255, nullable: true),
                    Email = table.Column<string>(type: "char(64)", maxLength: 64, nullable: true),
                    HasPublicPortfolio = table.Column<bool>(type: "bit", nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_HistoricalPositions_HistoricalPositionId",
                        column: x => x.HistoricalPositionId,
                        principalTable: "HistoricalPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Portfolios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Portfolios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Portfolios_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KnockoutPositions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortfolioId = table.Column<int>(type: "int", nullable: true),
                    KnockoutDerivativeId = table.Column<int>(type: "int", nullable: true),
                    NumberOfShares = table.Column<int>(type: "int", nullable: false),
                    BuyInPrice = table.Column<float>(type: "real", nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnockoutPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KnockoutPositions_KnockoutDerivatives_KnockoutDerivativeId",
                        column: x => x.KnockoutDerivativeId,
                        principalTable: "KnockoutDerivatives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KnockoutPositions_Portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WarrantPositions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortfolioId = table.Column<int>(type: "int", nullable: true),
                    WarrantId = table.Column<int>(type: "int", nullable: true),
                    BuyInPrice = table.Column<float>(type: "real", nullable: false),
                    NumberOfShares = table.Column<int>(type: "int", nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarrantPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarrantPositions_Portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarrantPositions_WarrantDerivatives_WarrantId",
                        column: x => x.WarrantId,
                        principalTable: "WarrantDerivatives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OngoingKnockoutPositions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KnockoutDerivativeId = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    PortfolioId = table.Column<int>(type: "int", nullable: true),
                    CurrentKnockoutPositionId = table.Column<int>(type: "int", nullable: true),
                    Price = table.Column<float>(type: "real", nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OngoingKnockoutPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OngoingKnockoutPositions_KnockoutDerivatives_KnockoutDerivativeId",
                        column: x => x.KnockoutDerivativeId,
                        principalTable: "KnockoutDerivatives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OngoingKnockoutPositions_KnockoutPositions_CurrentKnockoutPositionId",
                        column: x => x.CurrentKnockoutPositionId,
                        principalTable: "KnockoutPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OngoingKnockoutPositions_Portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OngoingWarrantPositions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WarrantDerivativeId = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    PortfolioId = table.Column<int>(type: "int", nullable: true),
                    CurrentWarrantPositionId = table.Column<int>(type: "int", nullable: true),
                    Price = table.Column<float>(type: "real", nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OngoingWarrantPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OngoingWarrantPositions_Portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OngoingWarrantPositions_WarrantDerivatives_WarrantDerivativeId",
                        column: x => x.WarrantDerivativeId,
                        principalTable: "WarrantDerivatives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OngoingWarrantPositions_WarrantPositions_CurrentWarrantPositionId",
                        column: x => x.CurrentWarrantPositionId,
                        principalTable: "WarrantPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoricalPositions_AssetInfoId",
                table: "HistoricalPositions",
                column: "AssetInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_KnockoutDerivatives_AssetInfoId",
                table: "KnockoutDerivatives",
                column: "AssetInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_KnockoutPositions_KnockoutDerivativeId",
                table: "KnockoutPositions",
                column: "KnockoutDerivativeId");

            migrationBuilder.CreateIndex(
                name: "IX_KnockoutPositions_PortfolioId",
                table: "KnockoutPositions",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_OngoingKnockoutPositions_CurrentKnockoutPositionId",
                table: "OngoingKnockoutPositions",
                column: "CurrentKnockoutPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_OngoingKnockoutPositions_KnockoutDerivativeId",
                table: "OngoingKnockoutPositions",
                column: "KnockoutDerivativeId");

            migrationBuilder.CreateIndex(
                name: "IX_OngoingKnockoutPositions_PortfolioId",
                table: "OngoingKnockoutPositions",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_OngoingWarrantPositions_CurrentWarrantPositionId",
                table: "OngoingWarrantPositions",
                column: "CurrentWarrantPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_OngoingWarrantPositions_PortfolioId",
                table: "OngoingWarrantPositions",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_OngoingWarrantPositions_WarrantDerivativeId",
                table: "OngoingWarrantPositions",
                column: "WarrantDerivativeId");

            migrationBuilder.CreateIndex(
                name: "IX_Portfolios_UserId",
                table: "Portfolios",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_HistoricalPositionId",
                table: "Users",
                column: "HistoricalPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantDerivatives_AssetInfoId",
                table: "WarrantDerivatives",
                column: "AssetInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantPositions_PortfolioId",
                table: "WarrantPositions",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantPositions_WarrantId",
                table: "WarrantPositions",
                column: "WarrantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OngoingKnockoutPositions");

            migrationBuilder.DropTable(
                name: "OngoingWarrantPositions");

            migrationBuilder.DropTable(
                name: "KnockoutPositions");

            migrationBuilder.DropTable(
                name: "WarrantPositions");

            migrationBuilder.DropTable(
                name: "KnockoutDerivatives");

            migrationBuilder.DropTable(
                name: "Portfolios");

            migrationBuilder.DropTable(
                name: "WarrantDerivatives");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "HistoricalPositions");

            migrationBuilder.DropTable(
                name: "AssetInfosSocieteGenerale");
        }
    }
}
