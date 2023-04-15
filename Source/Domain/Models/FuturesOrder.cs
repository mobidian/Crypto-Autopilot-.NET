using Bybit.Net.Enums;

namespace Domain.Models;

public class FuturesOrder : ICloneable
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
    
    public object Clone() => new FuturesOrder
    {
        BybitID = this.BybitID,
        CurrencyPair = (CurrencyPair)this.CurrencyPair.Clone(),
        CreateTime = this.CreateTime,
        UpdateTime = this.UpdateTime,
        Side = this.Side,
        PositionSide = this.PositionSide,
        Type = this.Type,
        Price = this.Price,
        Quantity = this.Quantity,
        StopLoss = this.StopLoss,
        TakeProfit = this.TakeProfit,
        TimeInForce = this.TimeInForce,
        Status = this.Status
    };
}
