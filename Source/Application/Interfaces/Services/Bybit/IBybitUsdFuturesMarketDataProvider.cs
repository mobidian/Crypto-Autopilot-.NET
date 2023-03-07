using Bybit.Net.Enums;
using Bybit.Net.Objects.Models;

namespace Application.Interfaces.Services.Bybit;

public interface IBybitUsdFuturesMarketDataProvider
{
    public Task<decimal> GetLastPriceAsync(string symbol);
    public Task<decimal?> GetMarketPriceAsync(string symbol);

    public Task<BybitTicker> GetTickerAsync(string symbol);

    public Task<IEnumerable<BybitKline>> GetAllCandlesticksAsync(string symbol, KlineInterval timeframe, int limit = 1000);

    public Task<IEnumerable<BybitKline>> GetCompletedCandlesticksAsync(string symbol, KlineInterval timeframe, int limit = 1000);
}
