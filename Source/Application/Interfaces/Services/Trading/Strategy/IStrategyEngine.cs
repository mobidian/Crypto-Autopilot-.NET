using Application.Interfaces.Services.Trading;
using Binance.Net.Objects.Models.Futures;
using Domain.Models;
using Skender.Stock.Indicators;

namespace Application.Interfaces.Services.Trading.Strategy;

public interface IStrategyEngine<T> : IDisposable where T : IQuote
{
    public ICfdTradingApiService ContractTrader { get; }

    /// <summary>
    /// // Reads and stores a given <see cref="T"/> array
    /// </summary>
    /// <param name="Candlesticks">The COMPLETED candlesticks</param>
    /// <param name="LastOpenPrice">The open price of the unfinished candlestick</param>
    public void SendData(T[] Candlesticks, decimal LastOpenPrice);

    /// <summary>
    /// Executes the strategy with respect to the previously given candlesticks array
    /// </summary>
    public void MakeMove();
}
