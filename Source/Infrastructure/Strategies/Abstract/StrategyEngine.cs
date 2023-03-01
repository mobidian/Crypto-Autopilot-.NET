using Application.Interfaces.Services.Trading;
using Application.Interfaces.Services.Trading.Monitors;

using Binance.Net.Enums;

using Domain.Models;

using MediatR;

namespace Infrastructure.Strategies.Abstract;

/// <summary>
/// The abstract base class for a cryptocurrency trading strategy engine
/// </summary>
public abstract class StrategyEngine : IStrategyEngine
{
    public Guid Guid { get; } = Guid.NewGuid();

    public CurrencyPair CurrencyPair { get; }
    public KlineInterval KlineInterval { get; }

    protected readonly IFuturesTradingService FuturesTrader;
    protected readonly IFuturesMarketDataProvider FuturesDataProvider;
    protected readonly IFuturesCandlesticksMonitor CandlestickMonitor;
    protected readonly IMediator Mediator;

    internal StrategyEngine(Guid guid, CurrencyPair currencyPair, KlineInterval klineInterval)
    {
        this.Guid = guid;
        this.CurrencyPair = currencyPair;
        this.KlineInterval = klineInterval;
    }
    protected StrategyEngine(CurrencyPair currencyPair, KlineInterval klineInterval, IFuturesTradingService futuresTrader, IFuturesMarketDataProvider futuresDataProvider, IFuturesCandlesticksMonitor candlestickMonitor, IMediator mediator)
    {
        const string exceptionMessage = $"Unable to initialize an object of type {nameof(StrategyEngine)} with a NULL parameter";

        this.CurrencyPair = currencyPair ?? throw new ArgumentNullException(nameof(currencyPair), exceptionMessage);
        this.KlineInterval = klineInterval;
        this.FuturesTrader = futuresTrader ?? throw new ArgumentNullException(nameof(futuresTrader), exceptionMessage);
        this.FuturesDataProvider = futuresDataProvider ?? throw new ArgumentNullException(nameof(futuresDataProvider), exceptionMessage);
        this.CandlestickMonitor = candlestickMonitor ?? throw new ArgumentNullException(nameof(candlestickMonitor), exceptionMessage);
        this.Mediator = mediator ?? throw new ArgumentNullException(nameof(mediator), exceptionMessage);
    }

    //// //// ////

    private volatile bool ShouldContinue = true;
    private volatile bool Running = false;
    public bool IsRunning() => this.Running;

    public async Task StartTradingAsync()
    {
        if (this.Running)
            return;


        await this.CandlestickMonitor.SubscribeToKlineUpdatesAsync(this.CurrencyPair.Name, ContractType.Perpetual, this.KlineInterval);
        this.Running = true;

        while (this.ShouldContinue)
        {
            await this.MakeMoveAsync();
        }

        if (this.ShouldContinue && this.FuturesTrader.IsInPosition())
            await this.FuturesTrader.ClosePositionAsync();

        this.Running = false;
    }

    /// <summary>
    /// <para>Represents the logic for executing a market move based on the current market data and signals provided by the client</para>
    /// <para>This method must be overridden by derived classes to provide the specific implementation of the trading strategy</para>
    /// </summary>
    /// <returns>A task that represents the execution of the method</returns>
    internal abstract Task MakeMoveAsync();

    public async Task StopTradingAsync()
    {
        this.ShouldContinue = false;

        while (this.Running)
        {
            await Task.Delay(20);
        }
    }


    //// //// ////


    private bool Disposed;

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (this.Disposed)
            return;

        if (disposing)
        {
            this.StopTradingAsync().GetAwaiter().GetResult();
            this.DisposeProperties();
        }

        this.Disposed = true;
    }

    public async ValueTask DisposeAsync()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        await DisposeAsync(disposing: true);
        GC.SuppressFinalize(this);
    }
    protected virtual async Task DisposeAsync(bool disposing)
    {
        if (this.Disposed)
            return;

        if (disposing)
        {
            await this.StopTradingAsync();
            this.DisposeProperties();
        }

        this.Disposed = true;
    }

    private void DisposeProperties()
    {
        this.FuturesTrader.Dispose();
        this.FuturesDataProvider.Dispose();
        this.FuturesDataProvider.Dispose();
    }
}
