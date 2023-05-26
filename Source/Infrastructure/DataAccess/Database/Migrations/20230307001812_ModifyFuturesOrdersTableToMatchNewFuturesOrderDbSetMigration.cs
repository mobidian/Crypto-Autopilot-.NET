using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModifyFuturesOrdersTableToMatchNewFuturesOrderDbSetMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FuturesOrders_Binance ID",
                table: "FuturesOrders");

            migrationBuilder.DropColumn(
                name: "AvgPrice",
                table: "FuturesOrders");

            migrationBuilder.DropColumn(
                name: "Binance ID",
                table: "FuturesOrders");

            migrationBuilder.DropColumn(
                name: "Order Working Type",
                table: "FuturesOrders");

            migrationBuilder.DropColumn(
                name: "PriceProtect",
                table: "FuturesOrders");

            migrationBuilder.RenameColumn(
                name: "StopPrice",
                table: "FuturesOrders",
                newName: "TakeProfit");

            migrationBuilder.AlterColumn<string>(
                name: "Order Status",
                table: "FuturesOrders",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(16)",
                oldMaxLength: 16);

            migrationBuilder.AddColumn<decimal>(
                name: "StopLoss",
                table: "FuturesOrders",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Unique ID",
                table: "FuturesOrders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_FuturesOrders_Unique ID",
                table: "FuturesOrders",
                column: "Unique ID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FuturesOrders_Unique ID",
                table: "FuturesOrders");

            migrationBuilder.DropColumn(
                name: "StopLoss",
                table: "FuturesOrders");

            migrationBuilder.DropColumn(
                name: "Unique ID",
                table: "FuturesOrders");

            migrationBuilder.RenameColumn(
                name: "TakeProfit",
                table: "FuturesOrders",
                newName: "StopPrice");

            migrationBuilder.AlterColumn<string>(
                name: "Order Status",
                table: "FuturesOrders",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32);

            migrationBuilder.AddColumn<decimal>(
                name: "AvgPrice",
                table: "FuturesOrders",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "Binance ID",
                table: "FuturesOrders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

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

            migrationBuilder.CreateIndex(
                name: "IX_FuturesOrders_Binance ID",
                table: "FuturesOrders",
                column: "Binance ID",
                unique: true);
        }
    }
}
