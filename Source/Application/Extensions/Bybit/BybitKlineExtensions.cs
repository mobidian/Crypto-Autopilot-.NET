using Bybit.Net.Objects.Models;

using Domain.Models.Common;

namespace Application.Extensions.Bybit;

public static class BybitKlineExtensions
{
    /// <summary>
    /// Converts a <see cref="BybitKline"/> object to a <see cref="Candlestick"/> object.
    /// </summary>
    /// <param name="bybitKline">The <see cref="BybitKline"/> object to convert.</param>
    public static Candlestick ToCandlestick(this BybitKline bybitKline) => new Candlestick
    {
        CurrencyPair = bybitKline.Symbol,
        Date = bybitKline.OpenTime,
        Open = bybitKline.OpenPrice,
        High = bybitKline.HighPrice,
        Low = bybitKline.LowPrice,
        Close = bybitKline.ClosePrice,
        Volume = bybitKline.Volume
    };

    /// <summary>
    /// Converts a collection of <see cref="BybitKline"/> objects to a collection of <see cref="Candlestick"/> objects.
    /// </summary>
    /// <param name="bybitKlines">The collection of <see cref="BybitKline"/> objects to convert.</param>
    public static IEnumerable<Candlestick> ToCandlesticks(this IEnumerable<BybitKline> bybitKlines) => bybitKlines.Select(x => x.ToCandlestick());
}
