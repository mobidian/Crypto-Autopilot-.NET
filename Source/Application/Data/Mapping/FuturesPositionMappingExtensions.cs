using Application.Data.Entities;

using Domain.Models;

namespace Application.Data.Mapping;

public static class FuturesPositionMappingExtensions
{
    public static FuturesPositionDbEntity ToDbEntity(this FuturesPosition position) => new FuturesPositionDbEntity
    {
        CurrencyPair = position.CurrencyPair.Name,
        Side = position.Side,
        Margin = position.Margin,
        Leverage = position.Leverage,
        Quantity = position.Quantity,
        EntryPrice = position.EntryPrice,
        ExitPrice = position.ExitPrice,
    };
    
    public static FuturesPosition ToDomainObject(this FuturesPositionDbEntity entity) => new FuturesPosition
    {
        CurrencyPair = entity.CurrencyPair,
        Side = entity.Side,
        Margin = entity.Margin,
        Leverage = entity.Leverage,
        Quantity = entity.Quantity,
        EntryPrice = entity.EntryPrice,
        ExitPrice = entity.ExitPrice,
    };
}
