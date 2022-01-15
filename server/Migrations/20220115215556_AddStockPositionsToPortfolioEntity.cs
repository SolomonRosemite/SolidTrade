using Microsoft.EntityFrameworkCore.Migrations;

namespace SolidTradeServer.Migrations
{
    public partial class AddStockPositionsToPortfolioEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockPositions_Portfolios_PortfolioId",
                table: "StockPositions");

            migrationBuilder.AlterColumn<int>(
                name: "PortfolioId",
                table: "StockPositions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StockPositions_Portfolios_PortfolioId",
                table: "StockPositions",
                column: "PortfolioId",
                principalTable: "Portfolios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockPositions_Portfolios_PortfolioId",
                table: "StockPositions");

            migrationBuilder.AlterColumn<int>(
                name: "PortfolioId",
                table: "StockPositions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_StockPositions_Portfolios_PortfolioId",
                table: "StockPositions",
                column: "PortfolioId",
                principalTable: "Portfolios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
