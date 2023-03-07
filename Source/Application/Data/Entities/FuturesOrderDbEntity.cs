using Application.Data.Entities.Common;

using Bybit.Net.Enums;

namespace Application.Data.Entities;

public class FuturesOrderDbEntity : BaseEntity
{
    public int CandlestickId { get; set; }
    public virtual CandlestickDbEntity Candlestick { get; set; } = default!;
    
    public required Guid UniqueID { get; set; }
    public required DateTime CreateTime { get; set; }
    public required DateTime UpdateTime { get; set; }
    public required OrderSide Side { get; set; }
    public required PositionSide PositionSide { get; set; }
    public required OrderType Type { get; set; }
    public required decimal Price { get; set; }
    public required decimal Quantity { get; set; }
    public required decimal? StopLoss { get; set; }
    public required decimal? TakeProfit { get; set; }
    public required TimeInForce TimeInForce { get; set; }
    public required OrderStatus Status { get; set; }
}
