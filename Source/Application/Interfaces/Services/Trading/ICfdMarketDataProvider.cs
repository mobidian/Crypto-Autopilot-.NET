using Binance.Net.Enums;

using Domain.Models;

namespace Application.Interfaces.Services.Trading;

/// <summary>
/// A provider that provides market data
/// </summary>
public interface ICfdMarketDataProvider : IDisposable
{
    public Task<decimal> GetCurrentPriceAsync(string currencyPair);

    /// <summary>
    /// Gets all of the candlesticks of the futures contract
    /// </summary>
    /// <returns></returns>
    public Task<IEnumerable<Candlestick>> GetAllCandlesticksAsync(string currencyPair, KlineInterval timeframe);

    /// <summary>
    /// Gets the completed candlesticks of the futures contract (skips the last candlestick)
    /// </summary>
    /// <param name="timeframe"></param>
    /// <returns></returns>
    public Task<IEnumerable<Candlestick>> GetCompletedCandlesticksAsync(string currencyPair, KlineInterval timeframe);
}
