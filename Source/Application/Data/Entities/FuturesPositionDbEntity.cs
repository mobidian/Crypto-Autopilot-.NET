using Application.Data.Entities.Common;

using Bybit.Net.Enums;

namespace Application.Data.Entities;

public class FuturesPositionDbEntity : BaseEntity
{
    public required Guid CryptoAutopilotId { get; init; }
    public required string CurrencyPair { get; set; }
    public required PositionSide Side { get; set; }
    public required decimal Margin { get; set; }
    public required decimal Leverage { get; set; }
    public required decimal Quantity { get; set; }
    public required decimal EntryPrice { get; set; }
    public required decimal? ExitPrice { get; set; }
    
    public IEnumerable<FuturesOrderDbEntity> FuturesOrders { get; set; } = default!;
}
