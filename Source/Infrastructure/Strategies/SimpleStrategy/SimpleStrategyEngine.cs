using Application.Interfaces.Services.Trading;
using Application.Interfaces.Services.Trading.Strategy;

using Binance.Net.Enums;

using Domain.Models;
using Infrastructure.Services.Trading;
using Infrastructure.Strategies.SimpleStrategy.Notifications;

using MediatR;

namespace Infrastructure.Strategies.SimpleStrategy;

/// <summary>
/// A simple strategy that opens a long position when <see cref="CFDMovingUp"/> is called and closes the position if it exists when <see cref="CFDMovingDown"/> is called
/// </summary>
public sealed class SimpleStrategyEngine : IStrategyEngine
{
    public CurrencyPair CurrencyPair { get; }
    public KlineInterval KlineInterval { get; }
    
    private readonly ICfdTradingService FuturesTrader;
    private readonly ICfdMarketDataProvider FuturesDataProvider;
    private readonly IFuturesMarketsCandlestickAwaiter CandlestickAwaiter;
    private readonly IMediator Mediator;
    
    public SimpleStrategyEngine(CurrencyPair currencyPair, KlineInterval klineInterval, ICfdTradingService futuresTrader, ICfdMarketDataProvider futuresDataProvider, IFuturesMarketsCandlestickAwaiter candlestickAwaiter, IMediator mediator)
    {
        this.CurrencyPair = currencyPair;
        this.KlineInterval = klineInterval;
        this.FuturesTrader = futuresTrader;
        this.FuturesDataProvider = futuresDataProvider;
        this.CandlestickAwaiter = candlestickAwaiter;
        this.Mediator = mediator;
    }

    //// //// ////

    private volatile bool ShouldContinue = true;
    private volatile bool Stopped = false;
    
    public async Task StartTradingAsync()
    {
        await this.CandlestickAwaiter.SubscribeToKlineUpdatesAsync();

        while (this.ShouldContinue)
        {
            await this.CandlestickAwaiter.WaitForNextCandlestickAsync();
            await this.MakeMoveAsync();
        }

        this.Stopped = true;
    }
    internal async Task MakeMoveAsync()
    {
        if (this.Signal is null)
            return;
        
        if (this.Signal == TradingviewSignal.Up && !this.FuturesTrader.IsInPosition())
        {
            await this.OpenPositionAsync();
        }
        else if (this.Signal == TradingviewSignal.Down && this.FuturesTrader.IsInPosition())
        {
            await this.ClosePositionAsync();
        }
    }
    private async Task OpenPositionAsync()
    {
        var price = await this.FuturesTrader.GetCurrentPriceAsync();
        await this.FuturesTrader.OpenPositionAtMarketPriceAsync(OrderSide.Buy, 20, 0.99m * price, 1.01m * price);
        
        var candlesticks = await this.FuturesDataProvider.GetCompletedCandlesticksAsync(this.KlineInterval);
        await this.Mediator.Publish(new PositionOpenedNotification(candlesticks.Last(), this.FuturesTrader.Position!));
    }
    private async Task ClosePositionAsync()
    {
        var closingOrder = await this.FuturesTrader.ClosePositionAsync();
        
        var candlesticks = await this.FuturesDataProvider.GetCompletedCandlesticksAsync(this.KlineInterval);
        await this.Mediator.Publish(new PositionClosedNotification(candlesticks.Last(), closingOrder));
    }
    
    #region Premium indicators endpoints
    private enum TradingviewSignal { Up, Down }
    private TradingviewSignal? Signal = null;
    
    public void CFDMovingUp() => this.Signal = TradingviewSignal.Up;
    public void CFDMovingDown() => this.Signal = TradingviewSignal.Down;
    #endregion

    public async Task StopTradingAsync()
    {
        this.ShouldContinue = false;

        while (!this.Stopped)
        {
            await Task.Delay(20);
        }
    }

    //// //// ////    

    public void Dispose()
    {
        this.StopTradingAsync().GetAwaiter().GetResult();
        this.DisposeProperties();
    }
    public async ValueTask DisposeAsync()
    {
        await this.StopTradingAsync();
        this.DisposeProperties();
    }

    private void DisposeProperties()
    {
        try
        {
            this.FuturesTrader.Dispose();
            this.FuturesDataProvider.Dispose();
        }
        finally { GC.SuppressFinalize(this); }
    }
}
