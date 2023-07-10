using Domain.Models.Common;

using Strategies.LuxAlgoImbalance.Enums;
using Strategies.LuxAlgoImbalance.Interfaces.Services;
using Strategies.LuxAlgoImbalance.Models;

namespace Strategies.LuxAlgoImbalance.Services;

public class FairValueGapFinder : IFairValueGapFinder
{
    public LuxAlgoFVG? FindLast(IEnumerable<Candlestick> candlesticks)
    {
        var candles = candlesticks.ToArray();

        for (var i = candles.Length - 1; i >= 2; i--)
        {
            var fvg = GetFvgOrNull(candles[i - 2], candles[i - 1], candles[i]);

            if (fvg is not null)
                return fvg;
        }

        return null;
    }
    private static LuxAlgoFVG? GetFvgOrNull(Candlestick first, Candlestick second, Candlestick third)
    {
        var isBullishFVG = second.IsBullish && first.High < third.Low;
        var isBearishFVG = second.IsBearish && first.Low > third.High;

        if (isBullishFVG || isBearishFVG)
        {
            return new LuxAlgoFVG
            {
                Side = second.IsBullish ? FvgSide.Bullish : FvgSide.Bearish,
                Top = second.IsBullish ? first.High : first.Low,
                Bottom = second.IsBullish ? third.Low : third.High
            };
        }
        else
            return null;
    }
}
