using Skender.Stock.Indicators;

namespace Application.Interfaces.Services.Trading;

/// <summary>
/// A service that interacts with the provided market data mapped as the specified generic type parameter
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IChartDataService<T> : IDisposable where T : IQuote
{
    /// <summary>
    /// Starts and returns the task of waiting for the current work in progress candle to be completed
    /// </summary>
    /// <returns></returns>
    public Task<T> WaitForNextCandleAsync();
}
