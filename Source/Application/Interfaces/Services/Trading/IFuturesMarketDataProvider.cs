using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;

using Domain.Models;

namespace Application.Interfaces.Services.Trading;

/// <summary>
/// Interface for getting data from Binance USDⓈ-M Futures markets
/// </summary>
public interface IFuturesMarketDataProvider : IDisposable
{
    /// <summary>
    /// Gets the current price for the specified contract as an asynchronous operation
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public Task<decimal> GetCurrentPriceAsync(string currencyPair);

    /// <summary>
    /// Gets the order details with the specified currency pair and Binance ID as an asynchronous operation
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public Task<BinanceFuturesOrder> GetOrderAsync(string currencyPair, long orderID);

    /// <summary>
    /// Gets all of the candlesticks of the futures contract as an asynchronous operation
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public Task<IEnumerable<Candlestick>> GetAllCandlesticksAsync(string currencyPair, KlineInterval timeframe);
    
    /// <summary>
    /// Gets the completed candlesticks of the futures contract (skips the last candlestick) as an asynchronous operation
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public Task<IEnumerable<Candlestick>> GetCompletedCandlesticksAsync(string currencyPair, KlineInterval timeframe);
}
