using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfiguredForeignKeysDeleteBehaviorToRestrictMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FuturesOrders_Candlesticks_Candlestick Id",
                table: "FuturesOrders");

            migrationBuilder.AddForeignKey(
                name: "FK_FuturesOrders_Candlesticks_Candlestick Id",
                table: "FuturesOrders",
                column: "Candlestick Id",
                principalTable: "Candlesticks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FuturesOrders_Candlesticks_Candlestick Id",
                table: "FuturesOrders");

            migrationBuilder.AddForeignKey(
                name: "FK_FuturesOrders_Candlesticks_Candlestick Id",
                table: "FuturesOrders",
                column: "Candlestick Id",
                principalTable: "Candlesticks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
