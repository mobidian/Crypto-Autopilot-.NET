using Application.Data.Entities;

using Binance.Net.Objects.Models.Futures;

namespace Application.Mapping;

public static class FuturesOrderMappingExtensions
{
    public static FuturesOrderDbEntity ToDbEntity(this BinanceFuturesOrder futuresOrder) => new FuturesOrderDbEntity
    {
        BinanceID = futuresOrder.Id,
        CreateTime = futuresOrder.CreateTime,
        UpdateTime = futuresOrder.UpdateTime,
        OrderSide = futuresOrder.Side,
        OrderType = futuresOrder.Type,
        OrderWorkingType = futuresOrder.WorkingType,
        Price = futuresOrder.Price,
        AvgPrice = futuresOrder.AvgPrice,
        StopPrice = futuresOrder.StopPrice,
        Quantity = futuresOrder.Quantity,
        PriceProtect = futuresOrder.PriceProtect,
        TimeInForce = futuresOrder.TimeInForce,
        OrderStatus = futuresOrder.Status,
    };

    public static BinanceFuturesOrder ToDomainObject(this FuturesOrderDbEntity entity) => new BinanceFuturesOrder
    {
        Id = entity.BinanceID,
        CreateTime = entity.CreateTime,
        UpdateTime = entity.UpdateTime,
        Side = entity.OrderSide,
        Type = entity.OrderType,
        WorkingType = entity.OrderWorkingType,
        Price = entity.Price,
        AvgPrice = entity.AvgPrice,
        StopPrice = entity.StopPrice,
        Quantity = entity.Quantity,
        PriceProtect = entity.PriceProtect,
        TimeInForce = entity.TimeInForce,
        Status = entity.OrderStatus
    };
}
