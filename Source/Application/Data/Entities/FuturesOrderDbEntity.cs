using Application.Data.Entities.Common;
using Binance.Net.Enums;

namespace Application.Data.Entities;

public class FuturesOrderDbEntity : BaseEntity
{
    public int CandlestickId { get; set; }
    public virtual CandlestickDbEntity Candlestick { get; set; } = default!;

    public long BinanceID { get; set; }
    public DateTime CreateTime { get; set; }
    public OrderSide OrderSide { get; set; }
    public FuturesOrderType OrderType { get; set; }
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
}
