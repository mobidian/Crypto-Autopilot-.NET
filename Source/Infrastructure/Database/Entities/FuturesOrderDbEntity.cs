using Binance.Net.Enums;

using Infrastructure.Database.Entities.Common;

namespace Infrastructure.Database.Entities;

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
