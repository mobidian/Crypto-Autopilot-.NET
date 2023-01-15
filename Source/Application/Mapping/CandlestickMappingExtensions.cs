using Application.Data.Entities;

using Domain.Models;

namespace Application.Mapping;

public static class CandlestickMappingExtensions
{
    public static CandlestickDbEntity ToDbEntity(this Candlestick candlestick) => new CandlestickDbEntity
    {
        BaseCurrency = candlestick.CurrencyPair.Base,
        QuoteCurrency = candlestick.CurrencyPair.Quote,
        DateTime = candlestick.Date,
        Open = candlestick.Open,
        High = candlestick.High,
        Low = candlestick.Low,
        Close = candlestick.Close,
        Volume = candlestick.Volume,
    };
    
    public static Candlestick ToDomainObject(this CandlestickDbEntity entity) => new Candlestick
    {
        CurrencyPair = new CurrencyPair(entity.BaseCurrency, entity.QuoteCurrency),
        Date = entity.DateTime,
        Open = entity.Open,
        High = entity.High,
        Low = entity.Low,
        Close = entity.Close,
        Volume = entity.Volume,
    };
}
