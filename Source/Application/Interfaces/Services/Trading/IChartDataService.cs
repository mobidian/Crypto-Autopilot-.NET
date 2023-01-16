using Domain.Models;

using Skender.Stock.Indicators;

namespace Application.Interfaces.Services.Trading;

/// <summary>
/// A service that provides market data and maps the candlesticks as a specified generic type
/// </summary>
/// <typeparam name="T">The type that the candlesticks will be mapped as</typeparam>
public interface IChartDataService<T> : IDisposable where T : IQuote
{
    /// <summary>
    /// Gets all of the completed candlesticks
    /// </summary>
    public T[] Candlesticks { get; }

    /// <summary>
    /// Starts and returns the task of waiting for the current work in progress candle to be completed
    /// </summary>
    /// <returns></returns>
    public Task<T> WaitForNextCandleAsync();

    public Task<T> GetUnfinishedCandlestickAsync();
    
    public Task<T> GetLastFinishedCandlestickAsync();

    public Task RegisterAllCandlesticksAsync();
}
