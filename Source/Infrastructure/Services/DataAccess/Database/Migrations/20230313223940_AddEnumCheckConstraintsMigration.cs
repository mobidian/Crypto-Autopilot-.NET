using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEnumCheckConstraintsMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_PositionSide",
                table: "FuturesPositions",
                sql: "[Side] IN ('Buy', 'Sell', 'None')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderPositionSide",
                table: "FuturesOrders",
                sql: "[Position Side] IN ('Buy', 'Sell', 'None')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderSide",
                table: "FuturesOrders",
                sql: "[Order Side] IN ('Buy', 'Sell')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderStatus",
                table: "FuturesOrders",
                sql: "[Order Status] IN ('Created', 'Rejected', 'New', 'PartiallyFilled', 'Filled', 'Canceled', 'PendingCancel', 'PartiallyFilledCanceled', 'UnTriggered')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrderType",
                table: "FuturesOrders",
                sql: "[Order Type] IN ('Limit', 'Market', 'LimitMaker')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TimeInForce",
                table: "FuturesOrders",
                sql: "[Time in force] IN ('GoodTillCanceled', 'ImmediateOrCancel', 'FillOrKill', 'PostOnly')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_PositionSide",
                table: "FuturesPositions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderPositionSide",
                table: "FuturesOrders");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderSide",
                table: "FuturesOrders");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderStatus",
                table: "FuturesOrders");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrderType",
                table: "FuturesOrders");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TimeInForce",
                table: "FuturesOrders");
        }
    }
}
