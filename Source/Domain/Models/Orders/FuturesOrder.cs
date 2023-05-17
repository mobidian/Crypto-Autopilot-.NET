using Bybit.Net.Enums;

using Domain.Models.Common;

namespace Domain.Models.Orders;

public class FuturesOrder
{
    public required Guid BybitID { get; init; }
    public required CurrencyPair CurrencyPair { get; init; }
    public required DateTime CreateTime { get; init; }
    public required DateTime UpdateTime { get; init; }
    public required OrderSide Side { get; init; }
    public required PositionSide PositionSide { get; init; }
    public required OrderType Type { get; init; }
    public required decimal Price { get; init; }
    public required decimal Quantity { get; init; }
    public required decimal? StopLoss { get; init; }
    public required decimal? TakeProfit { get; init; }
    public required TimeInForce TimeInForce { get; init; }
    public required OrderStatus Status { get; init; }
}
