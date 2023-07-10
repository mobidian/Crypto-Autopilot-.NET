using Domain.Models.Common;

using Strategies.LuxAlgoImbalance.Models;

namespace Strategies.LuxAlgoImbalance.Interfaces.Services;

public interface IFairValueGapFinder
{
    public LuxAlgoFVG? FindLast(IEnumerable<Candlestick> candlesticks);
}
