using Domain.Models;

using Skender.Stock.Indicators;

namespace Application.Interfaces.Services.Trading;

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

    public Task<decimal> GetUnfinishedCandlestickAsync();

    public Task<decimal> GetLastFinishedCandlestickAsync();

    public Task RegisterAllCandlesticksAsync();

    public void Quit();
}
