using Application.Data.Entities.Common;
using Binance.Net.Enums;

namespace Application.Data.Entities;

public class FuturesOrderDbEntity : BaseEntity
{
    public int CandlestickId { get; set; }
    public virtual CandlestickDbEntity Candlestick { get; set; } = default!;

    public required long BinanceID { get; set; }
    public required DateTime CreateTime { get; set; }
    public required OrderSide OrderSide { get; set; }
    public required FuturesOrderType OrderType { get; set; }
    public required decimal Price { get; set; }
    public required decimal Quantity { get; set; }
}
