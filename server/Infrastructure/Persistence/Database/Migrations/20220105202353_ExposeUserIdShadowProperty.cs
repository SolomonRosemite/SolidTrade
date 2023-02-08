﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace Infrastructure.Persistence.Database.Migrations;

public partial class ExposeUserIdShadowProperty : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<int>(
            name: "UserId",
            table: "HistoricalPositions",
            type: "int",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<int>(
            name: "UserId",
            table: "HistoricalPositions",
            type: "int",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int");
    }
}