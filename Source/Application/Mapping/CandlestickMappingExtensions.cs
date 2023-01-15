using Application.Data.Entities;

using Domain.Models;

namespace Application.Mapping;

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
    };
}
