using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SolidTradeServer.Migrations
{
    public partial class ChangeToTradeRepublicApi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HistoricalPositions_AssetInfosSocieteGenerale_AssetInfoId",
                table: "HistoricalPositions");

            migrationBuilder.DropForeignKey(
                name: "FK_KnockoutPositions_KnockoutDerivatives_KnockoutDerivativeId",
                table: "KnockoutPositions");

            migrationBuilder.DropForeignKey(
                name: "FK_OngoingKnockoutPositions_KnockoutDerivatives_KnockoutDerivativeId",
                table: "OngoingKnockoutPositions");

            migrationBuilder.DropForeignKey(
                name: "FK_OngoingWarrantPositions_WarrantDerivatives_WarrantDerivativeId",
                table: "OngoingWarrantPositions");

            migrationBuilder.DropForeignKey(
                name: "FK_WarrantPositions_WarrantDerivatives_WarrantId",
                table: "WarrantPositions");

            migrationBuilder.DropTable(
                name: "KnockoutDerivatives");

            migrationBuilder.DropTable(
                name: "WarrantDerivatives");

            migrationBuilder.DropTable(
                name: "AssetInfosSocieteGenerale");

            migrationBuilder.DropIndex(
                name: "IX_WarrantPositions_WarrantId",
                table: "WarrantPositions");

            migrationBuilder.DropIndex(
                name: "IX_OngoingWarrantPositions_WarrantDerivativeId",
                table: "OngoingWarrantPositions");

            migrationBuilder.DropIndex(
                name: "IX_OngoingKnockoutPositions_KnockoutDerivativeId",
                table: "OngoingKnockoutPositions");

            migrationBuilder.DropIndex(
                name: "IX_KnockoutPositions_KnockoutDerivativeId",
                table: "KnockoutPositions");

            migrationBuilder.DropIndex(
                name: "IX_HistoricalPositions_AssetInfoId",
                table: "HistoricalPositions");

            migrationBuilder.DropColumn(
                name: "WarrantId",
                table: "WarrantPositions");

            migrationBuilder.DropColumn(
                name: "WarrantDerivativeId",
                table: "OngoingWarrantPositions");

            migrationBuilder.DropColumn(
                name: "KnockoutDerivativeId",
                table: "OngoingKnockoutPositions");

            migrationBuilder.DropColumn(
                name: "KnockoutDerivativeId",
                table: "KnockoutPositions");

            migrationBuilder.DropColumn(
                name: "AssetInfoId",
                table: "HistoricalPositions");

            migrationBuilder.DropColumn(
                name: "LongOrShort",
                table: "HistoricalPositions");

            migrationBuilder.AddColumn<string>(
                name: "Isin",
                table: "WarrantPositions",
                type: "char(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Isin",
                table: "OngoingWarrantPositions",
                type: "char(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Isin",
                table: "OngoingKnockoutPositions",
                type: "char(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Isin",
                table: "KnockoutPositions",
                type: "char(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Isin",
                table: "HistoricalPositions",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Isin",
                table: "WarrantPositions");

            migrationBuilder.DropColumn(
                name: "Isin",
                table: "OngoingWarrantPositions");

            migrationBuilder.DropColumn(
                name: "Isin",
                table: "OngoingKnockoutPositions");

            migrationBuilder.DropColumn(
                name: "Isin",
                table: "KnockoutPositions");

            migrationBuilder.DropColumn(
                name: "Isin",
                table: "HistoricalPositions");

            migrationBuilder.AddColumn<int>(
                name: "WarrantId",
                table: "WarrantPositions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarrantDerivativeId",
                table: "OngoingWarrantPositions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KnockoutDerivativeId",
                table: "OngoingKnockoutPositions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KnockoutDerivativeId",
                table: "KnockoutPositions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssetInfoId",
                table: "HistoricalPositions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LongOrShort",
                table: "HistoricalPositions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AssetInfosSocieteGenerale",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    AssetImageUrl = table.Column<string>(type: "char(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Currency = table.Column<string>(type: "char(3)", maxLength: 3, nullable: true),
                    Name = table.Column<string>(type: "char(128)", maxLength: 128, nullable: true),
                    Ticker = table.Column<string>(type: "char(12)", maxLength: 12, nullable: true),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetInfosSocieteGenerale", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KnockoutDerivatives",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetInfoId = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "char(6)", maxLength: 6, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LongOrShort = table.Column<int>(type: "int", nullable: false),
                    StrikePrice = table.Column<float>(type: "real", nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
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
                    CallOrPut = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "char(6)", maxLength: 6, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpirationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    StrikePrice = table.Column<float>(type: "real", nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
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

            migrationBuilder.CreateIndex(
                name: "IX_WarrantPositions_WarrantId",
                table: "WarrantPositions",
                column: "WarrantId");

            migrationBuilder.CreateIndex(
                name: "IX_OngoingWarrantPositions_WarrantDerivativeId",
                table: "OngoingWarrantPositions",
                column: "WarrantDerivativeId");

            migrationBuilder.CreateIndex(
                name: "IX_OngoingKnockoutPositions_KnockoutDerivativeId",
                table: "OngoingKnockoutPositions",
                column: "KnockoutDerivativeId");

            migrationBuilder.CreateIndex(
                name: "IX_KnockoutPositions_KnockoutDerivativeId",
                table: "KnockoutPositions",
                column: "KnockoutDerivativeId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricalPositions_AssetInfoId",
                table: "HistoricalPositions",
                column: "AssetInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_KnockoutDerivatives_AssetInfoId",
                table: "KnockoutDerivatives",
                column: "AssetInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantDerivatives_AssetInfoId",
                table: "WarrantDerivatives",
                column: "AssetInfoId");

            migrationBuilder.AddForeignKey(
                name: "FK_HistoricalPositions_AssetInfosSocieteGenerale_AssetInfoId",
                table: "HistoricalPositions",
                column: "AssetInfoId",
                principalTable: "AssetInfosSocieteGenerale",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_KnockoutPositions_KnockoutDerivatives_KnockoutDerivativeId",
                table: "KnockoutPositions",
                column: "KnockoutDerivativeId",
                principalTable: "KnockoutDerivatives",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OngoingKnockoutPositions_KnockoutDerivatives_KnockoutDerivativeId",
                table: "OngoingKnockoutPositions",
                column: "KnockoutDerivativeId",
                principalTable: "KnockoutDerivatives",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OngoingWarrantPositions_WarrantDerivatives_WarrantDerivativeId",
                table: "OngoingWarrantPositions",
                column: "WarrantDerivativeId",
                principalTable: "WarrantDerivatives",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WarrantPositions_WarrantDerivatives_WarrantId",
                table: "WarrantPositions",
                column: "WarrantId",
                principalTable: "WarrantDerivatives",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
