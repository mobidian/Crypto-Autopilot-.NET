using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures.Socket;

namespace Application.Interfaces.Services.Trading.Monitors;

/// <summary>
/// Interface for monitoring Binance USDⓈ-M Futures candlestick updates
/// </summary>
public interface IFuturesCandlesticksMonitor : IDisposable
{
    /// <summary>
    /// Subscribes to Kline updates for a specific USDⓈ-M Futures contract
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public Task SubscribeToKlineUpdatesAsync(string currencyPair, ContractType contractType, KlineInterval timeframe);

    /// <summary>
    /// Determines whether the monitor is currently subscribed to Kline updates for a specific USDⓈ-M Futures contract
    /// </summary>
    /// <returns><see langword="true"/> if the monitor is subscribed to Kline updates for the specified USDⓈ-M Futures contract; otherwise <see langword="false"/></returns>
    public bool IsSubscribedTo(string currencyPair, ContractType contractType, KlineInterval timeframe);

    /// <summary>
    /// Returns an enumerable collection of tuples containing the data about the USDⓈ-M Futures contracts that the monitor is currently subscribed to
    /// </summary>
    /// <returns>An enumerable collection of currencyPair, contractType and KlineInterval value tuples </returns>
    public IEnumerable<(string currencyPair, ContractType contractType, KlineInterval timeframe)> GetSubscriptions();

    /// <summary>
    /// Unsubscribes from Kline updates for a specific USDⓈ-M Futures contract
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public Task UnsubscribeFromKlineUpdatesAsync(string currencyPair, ContractType contractType, KlineInterval timeframe);

    /// <summary>
    /// Asynchronously waits for the next candlestick update for a specific USDⓈ-M Futures contract
    /// </summary>
    /// <returns>
    /// <para>A task that represents the asynchronous operation</para>
    /// <para>The task result contains the next USDⓈ-M Futures candlestick data for the specified contract</para>
    /// </returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public Task<BinanceStreamContinuousKlineData> WaitForNextCandlestickAsync(string currencyPair, ContractType contractType, KlineInterval timeframe);
}
