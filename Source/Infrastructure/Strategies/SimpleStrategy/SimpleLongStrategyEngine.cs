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
    internal SimpleLongStrategyEngine(Guid guid, CurrencyPair currencyPair, KlineInterval klineInterval) : base(guid, currencyPair, klineInterval) { }
    public SimpleLongStrategyEngine(CurrencyPair currencyPair, KlineInterval klineInterval, decimal margin, decimal stopLossParameter, decimal takeProfitParameter, ICfdTradingService futuresTrader, ICfdMarketDataProvider futuresDataProvider, IFuturesMarketsCandlestickAwaiter candlestickAwaiter, IMediator mediator) : base(currencyPair, klineInterval, margin, stopLossParameter, takeProfitParameter, futuresTrader, futuresDataProvider, candlestickAwaiter, mediator)
    {
        if (this.StopLossParameter is >= 1 or <= 0)
            throw new ArgumentException($"When trading longs the {nameof(this.StopLossParameter)} must be between 0 and 1", nameof(this.StopLossParameter));

        if (this.TakeProfitParameter <= 1)
            throw new ArgumentException($"When trading longs the {nameof(this.TakeProfitParameter)} must be greater than 1", nameof(this.TakeProfitParameter));
    }
    
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
        await this.FuturesTrader.OpenPositionAtMarketPriceAsync(OrderSide.Buy, this.Margin, this.StopLossParameter * price, this.TakeProfitParameter * price);

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
