using Domain.Models.Common;

using Strategies.LuxAlgoImbalance.Models;

namespace Strategies.LuxAlgoImbalance;

public interface IFairValueGapFinder
{
    public LuxAlgoFVG? FindLast(IEnumerable<Candlestick> candlesticks);
}
