using Application.Data.Entities.Common;
using Binance.Net.Enums;

namespace Application.Data.Entities;

public class FuturesOrderDbEntity : BaseEntity
{
    public int CandlestickId { get; set; }
    public virtual CandlestickDbEntity Candlestick { get; set; } = default!;

    public required long BinanceID { get; set; }
    public required DateTime CreateTime { get; set; }
    public required DateTime UpdateTime { get; set; }
    public required OrderSide OrderSide { get; set; }
    public required FuturesOrderType OrderType { get; set; }
    public required WorkingType OrderWorkingType { get; set; }
    public required decimal Price { get; set; }
    public required decimal AvgPrice { get; set; }
    public required decimal? StopPrice { get; set; }
    public required decimal Quantity { get; set; }
    public required bool PriceProtect { get; set; }
    public required TimeInForce TimeInForce { get; set; }
    public required OrderStatus OrderStatus { get; set; }
}
