using Application.Interfaces.Services.Trading;
using Application.Interfaces.Services.Trading.Strategy;

using Binance.Net.Enums;

using Domain.Models;

using Infrastructure.Notifications;
using Infrastructure.Services.Trading;
using Infrastructure.Strategies.Abstract;

using MediatR;

namespace Infrastructure.Strategies.SimpleStrategy;

/// <summary>
/// A simple strategy that opens a long position when <see cref="CFDMovingUp"/> is called and closes the position if it exists when <see cref="CFDMovingDown"/> is called
/// </summary>
public sealed class SimpleLongStrategyEngine : SimpleStrategyEngine
{
    public SimpleLongStrategyEngine(CurrencyPair currencyPair, KlineInterval klineInterval, ICfdTradingService futuresTrader, ICfdMarketDataProvider futuresDataProvider, IFuturesMarketsCandlestickAwaiter candlestickAwaiter, IMediator mediator) : base(currencyPair, klineInterval, futuresTrader, futuresDataProvider, candlestickAwaiter, mediator) { }
    
    //// //// ////
    
    internal override async Task MakeMoveAsync()
    {
        if (this.Signal is null)
            return;

        if (this.Signal == TradingviewSignal.Up && !this.FuturesTrader.IsInPosition())
        {
            await this.OpenLongPositionAsync();
        }
        else if (this.Signal == TradingviewSignal.Down && this.FuturesTrader.IsInPosition())
        {
            await this.ClosePositionAsync();
        }

        this.Signal = null;
    }
    private async Task OpenLongPositionAsync()
    {
        var price = await this.FuturesTrader.GetCurrentPriceAsync();
        await this.FuturesTrader.OpenPositionAtMarketPriceAsync(OrderSide.Buy, 20, 0.99m * price, 1.01m * price);

        var candlesticks = await this.FuturesDataProvider.GetCompletedCandlesticksAsync(this.CurrencyPair.Name, this.KlineInterval);
        await this.Mediator.Publish(new PositionOpenedNotification(candlesticks.Last(), this.FuturesTrader.Position!));
    }
    private async Task ClosePositionAsync()
    {
        var closingOrder = await this.FuturesTrader.ClosePositionAsync();

        var candlesticks = await this.FuturesDataProvider.GetCompletedCandlesticksAsync(this.CurrencyPair.Name, this.KlineInterval);
        await this.Mediator.Publish(new PositionClosedNotification(candlesticks.Last(), closingOrder));
    }
}
