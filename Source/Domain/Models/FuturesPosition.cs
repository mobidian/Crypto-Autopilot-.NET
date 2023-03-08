using Bybit.Net.Enums;

namespace Domain.Models;

public class FuturesPosition
{
    public required CurrencyPair CurrencyPair { get; init; }
    public required PositionSide Side { get; init; }
    public required decimal Margin { get; init; }
    public required decimal Leverage { get; init; }
    public required decimal Quantity { get; init; }
    public required decimal EntryPrice { get; init; }
    public required decimal? ExitPrice { get; set; }
}
