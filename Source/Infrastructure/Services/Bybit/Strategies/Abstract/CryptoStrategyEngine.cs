using Application.Interfaces.Services;
using Application.Interfaces.Services.Bybit;

using Bybit.Net.Enums;

using Domain.Models;

using MediatR;

namespace Infrastructure.Services.Bybit.Strategies.Abstract;

public abstract class CryptoStrategyEngine : IStrategyEngine
{
    public Guid Guid { get; } = Guid.NewGuid();
    public CurrencyPair CurrencyPair { get; }

    protected readonly IBybitUsdFuturesMarketDataProvider MarketDataProvider;
    protected readonly IBybitUsdFuturesTradingService FuturesTrader;
    protected readonly IMediator Mediator;

    public CryptoStrategyEngine(CurrencyPair currencyPair, IBybitUsdFuturesMarketDataProvider marketDataProvider, IBybitUsdFuturesTradingService futuresTrader, IMediator mediator)
    {
        this.CurrencyPair = currencyPair ?? throw new ArgumentNullException(nameof(currencyPair));
        this.MarketDataProvider = marketDataProvider ?? throw new ArgumentNullException(nameof(marketDataProvider));
        this.FuturesTrader = futuresTrader ?? throw new ArgumentNullException(nameof(futuresTrader));
        this.Mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    //// //// ////

    private volatile bool ShouldContinue = true;
    private volatile bool Running = false;

    public bool IsRunning() => this.Running;

    public async Task StartTradingAsync()
    {
        if (this.Running)
            return;

        this.Running = true;
        while (this.ShouldContinue)
        {
            await this.MakeMoveAsync();
        }

        await ClosePositionsAndLimitOrdersAsync();

        this.Running = false;
    }
    private async Task ClosePositionsAndLimitOrdersAsync()
    {
        if (this.FuturesTrader.LongPosition is not null)
            await this.FuturesTrader.ClosePositionAsync(PositionSide.Buy);

        if (this.FuturesTrader.ShortPosition is not null)
            await this.FuturesTrader.ClosePositionAsync(PositionSide.Sell);

        if (this.FuturesTrader.BuyLimitOrder is not null)
            await this.FuturesTrader.CancelLimitOrderAsync(OrderSide.Buy);

        if (this.FuturesTrader.SellLimitOrder is not null)
            await this.FuturesTrader.CancelLimitOrderAsync(OrderSide.Buy);
    }

    protected internal abstract Task MakeMoveAsync();

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
            this.StopTradingAsync().GetAwaiter().GetResult();

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
            await this.StopTradingAsync();

        this.Disposed = true;
    }
}
