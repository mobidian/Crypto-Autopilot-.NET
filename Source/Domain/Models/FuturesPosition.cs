using Bybit.Net.Enums;

namespace Domain.Models;

public class FuturesPosition : ICloneable
{
    public required Guid CryptoAutopilotId { get; init; }
    public required CurrencyPair CurrencyPair { get; init; }
    public required PositionSide Side { get; init; }
    public required decimal Margin { get; init; }
    public required decimal Leverage { get; init; }
    public required decimal Quantity { get; init; }
    public required decimal EntryPrice { get; init; }
    public decimal? StopLoss { get; set; }
    public decimal? TakeProfit { get; set; }
    public decimal? ExitPrice { get; set; }
    
    public object Clone() => new FuturesPosition
    {
        CryptoAutopilotId = this.CryptoAutopilotId,
        CurrencyPair = (CurrencyPair)this.CurrencyPair.Clone(),
        Side = this.Side,
        Margin = this.Margin,
        Leverage = this.Leverage,
        Quantity = this.Quantity,
        EntryPrice = this.EntryPrice,
        StopLoss = this.StopLoss,
        TakeProfit = this.TakeProfit,
        ExitPrice = this.ExitPrice
    };
}
