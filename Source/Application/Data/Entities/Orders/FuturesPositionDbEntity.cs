using Application.Data.Entities.Common;

using Bybit.Net.Enums;

namespace Application.Data.Entities.Orders;

public class FuturesPositionDbEntity : DbEntityBase
{
    public required Guid CryptoAutopilotId { get; init; }
    public required string CurrencyPair { get; set; }
    public required PositionSide Side { get; set; }
    public required decimal Margin { get; set; }
    public required decimal Leverage { get; set; }
    public required decimal Quantity { get; set; }
    public required decimal EntryPrice { get; set; }
    public decimal? StopLoss { get; set; }
    public decimal? TakeProfit { get; set; }
    public decimal? ExitPrice { get; set; }

    public virtual IEnumerable<FuturesOrderDbEntity>? FuturesOrders { get; set; }
}
