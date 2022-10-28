﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace Infrastructure.Persistence.Database.Migrations;

public partial class AddUidToUser : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Portfolios_Users_UserId",
            table: "Portfolios");

        migrationBuilder.DropIndex(
            name: "IX_Portfolios_UserId",
            table: "Portfolios");

        migrationBuilder.DropColumn(
            name: "UserId",
            table: "Portfolios");

        migrationBuilder.AddColumn<int>(
            name: "PortfolioId",
            table: "Users",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<string>(
            name: "Uid",
            table: "Users",
            type: "char(128)",
            maxLength: 128,
            nullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "Balance",
            table: "Portfolios",
            type: "decimal(18,2)",
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.CreateIndex(
            name: "IX_Users_PortfolioId",
            table: "Users",
            column: "PortfolioId",
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_Users_Portfolios_PortfolioId",
            table: "Users",
            column: "PortfolioId",
            principalTable: "Portfolios",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Users_Portfolios_PortfolioId",
            table: "Users");

        migrationBuilder.DropIndex(
            name: "IX_Users_PortfolioId",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "PortfolioId",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "Uid",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "Balance",
            table: "Portfolios");

        migrationBuilder.AddColumn<int>(
            name: "UserId",
            table: "Portfolios",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateIndex(
            name: "IX_Portfolios_UserId",
            table: "Portfolios",
            column: "UserId",
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_Portfolios_Users_UserId",
            table: "Portfolios",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}