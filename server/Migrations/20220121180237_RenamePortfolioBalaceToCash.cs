using Microsoft.EntityFrameworkCore.Migrations;

namespace SolidTradeServer.Migrations
{
    public partial class RenamePortfolioBalaceToCash : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InitialBalance",
                table: "Portfolios",
                newName: "InitialCash");

            migrationBuilder.RenameColumn(
                name: "Balance",
                table: "Portfolios",
                newName: "Cash");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InitialCash",
                table: "Portfolios",
                newName: "InitialBalance");

            migrationBuilder.RenameColumn(
                name: "Cash",
                table: "Portfolios",
                newName: "Balance");
        }
    }
}
