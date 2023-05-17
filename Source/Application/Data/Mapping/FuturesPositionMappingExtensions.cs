using Application.Data.Entities.Orders;

using Domain.Models.Orders;

namespace Application.Data.Mapping;

public static class FuturesPositionMappingExtensions
{
    public static FuturesPositionDbEntity ToDbEntity(this FuturesPosition position) => new FuturesPositionDbEntity
    {
        CryptoAutopilotId = position.CryptoAutopilotId,
        CurrencyPair = position.CurrencyPair.Name,
        Side = position.Side,
        Margin = position.Margin,
        Leverage = position.Leverage,
        Quantity = position.Quantity,
        EntryPrice = position.EntryPrice,
        StopLoss = position.StopLoss,
        TakeProfit = position.TakeProfit,
        ExitPrice = position.ExitPrice,
    };
    
    public static FuturesPosition ToDomainObject(this FuturesPositionDbEntity entity) => new FuturesPosition
    {
        CryptoAutopilotId = entity.CryptoAutopilotId,
        CurrencyPair = entity.CurrencyPair,
        Side = entity.Side,
        Margin = entity.Margin,
        Leverage = entity.Leverage,
        Quantity = entity.Quantity,
        EntryPrice = entity.EntryPrice,
        StopLoss = entity.StopLoss,
        TakeProfit = entity.TakeProfit,
        ExitPrice = entity.ExitPrice,
    };
}
