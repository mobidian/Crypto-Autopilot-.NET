using Application.Data.Entities;

using Domain.Models;

namespace Application.Data.Mapping;

public static class FuturesOrderMappingExtensions
{
    public static FuturesOrderDbEntity ToDbEntity(this FuturesOrder futuresOrder) => new FuturesOrderDbEntity
    {
        UniqueID = futuresOrder.UniqueID,
        CreateTime = futuresOrder.CreateTime,
        UpdateTime = futuresOrder.UpdateTime,
        Side = futuresOrder.Side,
        PositionSide = futuresOrder.PositionSide,
        Type = futuresOrder.Type,
        Price = futuresOrder.Price,
        Quantity = futuresOrder.Quantity,
        StopLoss = futuresOrder.StopLoss!.Value,
        TakeProfit = futuresOrder.TakeProfit!.Value,
        TimeInForce = futuresOrder.TimeInForce,
        Status = futuresOrder.Status,
    };

    public static FuturesOrder ToDomainObject(this FuturesOrderDbEntity entity) => new FuturesOrder
    {
        UniqueID = entity.UniqueID,
        CurrencyPair = entity.Candlestick.CurrencyPair,
        CreateTime = entity.CreateTime,
        UpdateTime = entity.UpdateTime,
        Side = entity.Side,
        PositionSide = entity.PositionSide,
        Type = entity.Type,
        Price = entity.Price,
        Quantity = entity.Quantity,
        StopLoss = entity.StopLoss!.Value,
        TakeProfit = entity.TakeProfit!.Value,
        TimeInForce = entity.TimeInForce,
        Status = entity.Status,
    };
}
