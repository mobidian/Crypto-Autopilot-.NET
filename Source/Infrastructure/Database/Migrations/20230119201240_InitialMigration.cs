using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Candlesticks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CurrencyPair = table.Column<string>(name: "Currency Pair", type: "nvarchar(16)", maxLength: 16, nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Open = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    High = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Low = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Close = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Volume = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RecordCreatedDate = table.Column<DateTime>(name: "Record Created Date", type: "datetime2", maxLength: 50, nullable: false),
                    RecordModifiedDate = table.Column<DateTime>(name: "Record Modified Date", type: "datetime2", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candlesticks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FuturesOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandlestickId = table.Column<int>(name: "Candlestick Id", type: "int", nullable: false),
                    BinanceID = table.Column<long>(name: "Binance ID", type: "bigint", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderSide = table.Column<int>(name: "Order Side", type: "int", maxLength: 8, nullable: false),
                    OrderType = table.Column<int>(name: "Order Type", type: "int", maxLength: 32, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RecordCreatedDate = table.Column<DateTime>(name: "Record Created Date", type: "datetime2", maxLength: 50, nullable: false),
                    RecordModifiedDate = table.Column<DateTime>(name: "Record Modified Date", type: "datetime2", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuturesOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FuturesOrders_Candlesticks_Candlestick Id",
                        column: x => x.CandlestickId,
                        principalTable: "Candlesticks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Candlesticks_Currency Pair",
                table: "Candlesticks",
                column: "Currency Pair");

            migrationBuilder.CreateIndex(
                name: "IX_Candlesticks_Currency Pair_DateTime",
                table: "Candlesticks",
                columns: new[] { "Currency Pair", "DateTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FuturesOrders_Binance ID",
                table: "FuturesOrders",
                column: "Binance ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FuturesOrders_Candlestick Id",
                table: "FuturesOrders",
                column: "Candlestick Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FuturesOrders");

            migrationBuilder.DropTable(
                name: "Candlesticks");
        }
    }
}
