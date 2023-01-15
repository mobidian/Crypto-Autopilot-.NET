using Application.Data.Entities;

using Binance.Net.Objects.Models.Futures;

namespace Application.Mapping;

public static class FuturesOrderMappingExtensions
{
    public static FuturesOrderDbEntity ToDbEntity(this BinanceFuturesOrder futuresOrder) => new FuturesOrderDbEntity
    {
        BinanceID = futuresOrder.Id,
        CreateTime = futuresOrder.CreateTime,
        OrderSide = futuresOrder.Side,
        OrderType = futuresOrder.Type,
        Price = futuresOrder.Price,
        Quantity = futuresOrder.Quantity,
    };

    public static BinanceFuturesOrder ToDomainObject(this FuturesOrderDbEntity entity) => new BinanceFuturesOrder
    {
        Id = entity.BinanceID,
        CreateTime = entity.CreateTime,
        Side = entity.OrderSide,
        Type = entity.OrderType,
        Price = entity.Price,
        Quantity = entity.Quantity
    };
}
