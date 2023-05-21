using Bybit.Net.Enums;

using Domain.Models.Common;

namespace Domain.Models.Futures;

public class FuturesPosition
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
}
