using Binance.Net.Interfaces;

using Domain.Models;

namespace Infrastructure.Extensions;

public static class BinanceTypesToCandlestickExtensions
{
    public static Candlestick ToCandlestick(this IBinanceStreamKlineData streamKlineData)
    {
        _ = streamKlineData.Data ?? throw new NullReferenceException($"The {nameof(streamKlineData)} has no IBinanceStreamKline, {nameof(streamKlineData)}.Data was NULL");

        return new Candlestick
        {
            CurrencyPair = streamKlineData.Symbol,

            Date = streamKlineData.Data.OpenTime,

            Open = streamKlineData.Data.OpenPrice,
            High = streamKlineData.Data.HighPrice,
            Low = streamKlineData.Data.LowPrice,
            Close = streamKlineData.Data.ClosePrice,
            Volume = streamKlineData.Data.Volume,
        };
    }

    public static Candlestick ToCandlestick(this IBinanceKline kline, string CurrencyPair) => new Candlestick
    {
        CurrencyPair = CurrencyPair,

        Date = kline.OpenTime,

        Open = kline.OpenPrice,
        High = kline.HighPrice,
        Low = kline.LowPrice,
        Close = kline.ClosePrice,
        Volume = kline.Volume,
    };
}
