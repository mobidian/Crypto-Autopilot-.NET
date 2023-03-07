using Application.Data.Entities;

using Domain.Models;

namespace Application.Data.Mapping;

public static class CandlestickMappingExtensions
{
    public static CandlestickDbEntity ToDbEntity(this Candlestick candlestick) => new CandlestickDbEntity
    {
        CurrencyPair = candlestick.CurrencyPair.Name,
        DateTime = candlestick.Date,
        Open = candlestick.Open,
        High = candlestick.High,
        Low = candlestick.Low,
        Close = candlestick.Close,
        Volume = candlestick.Volume,
    };

    public static Candlestick ToDomainObject(this CandlestickDbEntity entity) => new Candlestick
    {
        CurrencyPair = new CurrencyPair(entity.CurrencyPair),
        Date = entity.DateTime,
        Open = entity.Open,
        High = entity.High,
        Low = entity.Low,
        Close = entity.Close,
        Volume = entity.Volume,
    };
}
