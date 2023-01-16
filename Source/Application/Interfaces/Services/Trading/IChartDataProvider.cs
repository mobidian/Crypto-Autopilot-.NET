using Skender.Stock.Indicators;

namespace Application.Interfaces.Services.Trading;

/// <summary>
/// A provider that provides market data and maps the candlesticks as a specified generic type
/// </summary>
/// <typeparam name="T">The type that the candlesticks will be mapped as</typeparam>
public interface IChartDataProvider<T> : IDisposable where T : IQuote
{
    /// <summary>
    /// Gets all of the completed candlesticks registered by the provider
    /// </summary>
    public T[] Candlesticks { get; }

    /// <summary>
    /// Gets the unfinished candlestick in the chart asynchronously
    /// </summary>
    /// <returns></returns>
    public Task<T> GetUnfinishedCandlestickAsync();
    
    /// <summary>
    /// Gets the last finised candlestick in the chart asynchronously
    /// </summary>
    /// <returns></returns>
    public Task<T> GetLastFinishedCandlestickAsync();
}
