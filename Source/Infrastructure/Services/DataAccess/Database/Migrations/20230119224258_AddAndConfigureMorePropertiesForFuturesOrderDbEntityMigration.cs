using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAndConfigureMorePropertiesForFuturesOrderDbEntityMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AvgPrice",
                table: "FuturesOrders",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Order Status",
                table: "FuturesOrders",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Order Working Type",
                table: "FuturesOrders",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "PriceProtect",
                table: "FuturesOrders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "StopPrice",
                table: "FuturesOrders",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Time in force",
                table: "FuturesOrders",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateTime",
                table: "FuturesOrders",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvgPrice",
                table: "FuturesOrders");

            migrationBuilder.DropColumn(
                name: "Order Status",
                table: "FuturesOrders");

            migrationBuilder.DropColumn(
                name: "Order Working Type",
                table: "FuturesOrders");

            migrationBuilder.DropColumn(
                name: "PriceProtect",
                table: "FuturesOrders");

            migrationBuilder.DropColumn(
                name: "StopPrice",
                table: "FuturesOrders");

            migrationBuilder.DropColumn(
                name: "Time in force",
                table: "FuturesOrders");

            migrationBuilder.DropColumn(
                name: "UpdateTime",
                table: "FuturesOrders");
        }
    }
}
