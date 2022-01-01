using Microsoft.EntityFrameworkCore.Migrations;

namespace SolidTradeServer.Migrations
{
    public partial class MakeHistoricalPositionOneToManyRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_HistoricalPositions_HistoricalPositionId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_HistoricalPositionId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "HistoricalPositionId",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "HistoricalPositions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HistoricalPositions_UserId",
                table: "HistoricalPositions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_HistoricalPositions_Users_UserId",
                table: "HistoricalPositions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HistoricalPositions_Users_UserId",
                table: "HistoricalPositions");

            migrationBuilder.DropIndex(
                name: "IX_HistoricalPositions_UserId",
                table: "HistoricalPositions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "HistoricalPositions");

            migrationBuilder.AddColumn<int>(
                name: "HistoricalPositionId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_HistoricalPositionId",
                table: "Users",
                column: "HistoricalPositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_HistoricalPositions_HistoricalPositionId",
                table: "Users",
                column: "HistoricalPositionId",
                principalTable: "HistoricalPositions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
