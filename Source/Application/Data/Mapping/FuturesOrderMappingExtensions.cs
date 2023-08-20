using Application.Data.Entities.Futures;

using Domain.Models.Futures;

namespace Application.Data.Mapping;

public static class FuturesOrderMappingExtensions
{
    public static FuturesOrderDbEntity ToDbEntity(this FuturesOrder futuresOrder) => new FuturesOrderDbEntity
    {
        BybitID = futuresOrder.BybitID,
        CurrencyPair = futuresOrder.CurrencyPair.Name,
        CreateTime = futuresOrder.CreateTime,
        UpdateTime = futuresOrder.UpdateTime,
        Side = futuresOrder.Side,
        PositionSide = futuresOrder.PositionSide,
        Type = futuresOrder.Type,
        Price = futuresOrder.Price,
        Quantity = futuresOrder.Quantity,
        StopLoss = futuresOrder.StopLoss,
        TakeProfit = futuresOrder.TakeProfit,
        TimeInForce = futuresOrder.TimeInForce,
        Status = futuresOrder.Status,
    };

    public static FuturesOrder ToDomainObject(this FuturesOrderDbEntity entity) => new FuturesOrder
    {
        BybitID = entity.BybitID,
        CurrencyPair = entity.CurrencyPair,
        CreateTime = entity.CreateTime,
        UpdateTime = entity.UpdateTime,
        Side = entity.Side,
        PositionSide = entity.PositionSide,
        Type = entity.Type,
        Price = entity.Price,
        Quantity = entity.Quantity,
        StopLoss = entity.StopLoss,
        TakeProfit = entity.TakeProfit,
        TimeInForce = entity.TimeInForce,
        Status = entity.Status,
    };
}
