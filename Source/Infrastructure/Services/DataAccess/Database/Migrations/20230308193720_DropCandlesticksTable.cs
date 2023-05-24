using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropCandlesticksTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FuturesOrders_Candlesticks_Candlestick Id",
                table: "FuturesOrders");

            migrationBuilder.DropTable(
                name: "Candlesticks");

            migrationBuilder.DropIndex(
                name: "IX_FuturesOrders_Candlestick Id",
                table: "FuturesOrders");

            migrationBuilder.DropColumn(
                name: "Candlestick Id",
                table: "FuturesOrders")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "FuturesOrdersHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AddColumn<string>(
                name: "CurrencyPair",
                table: "FuturesOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "FuturesOrdersHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrencyPair",
                table: "FuturesOrders")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "FuturesOrdersHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.AddColumn<int>(
                name: "Candlestick Id",
                table: "FuturesOrders",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "FuturesOrdersHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.CreateTable(
                name: "Candlesticks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Close = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CurrencyPair = table.Column<string>(name: "Currency Pair", type: "nvarchar(16)", maxLength: 16, nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    High = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Low = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Open = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Volume = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candlesticks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FuturesOrders_Candlestick Id",
                table: "FuturesOrders",
                column: "Candlestick Id");

            migrationBuilder.CreateIndex(
                name: "IX_Candlesticks_Currency Pair",
                table: "Candlesticks",
                column: "Currency Pair");

            migrationBuilder.CreateIndex(
                name: "IX_Candlesticks_Currency Pair_DateTime",
                table: "Candlesticks",
                columns: new[] { "Currency Pair", "DateTime" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FuturesOrders_Candlesticks_Candlestick Id",
                table: "FuturesOrders",
                column: "Candlestick Id",
                principalTable: "Candlesticks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
