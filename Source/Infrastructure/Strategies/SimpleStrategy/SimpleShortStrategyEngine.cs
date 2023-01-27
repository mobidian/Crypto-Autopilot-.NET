using Application.Interfaces.Services.Trading;

using Binance.Net.Enums;

using Domain.Models;

using Infrastructure.Notifications;
using Infrastructure.Services.Trading;

using MediatR;

namespace Infrastructure.Strategies.SimpleStrategy;

/// <summary>
/// A simple strategy that opens a short position when <see cref="CFDMovingDown"/> is called and closes the position if it exists when <see cref="CFDMovingUp"/> is called
/// </summary>
public sealed class SimpleShortStrategyEngine : SimpleStrategyEngine
{
    internal SimpleShortStrategyEngine(Guid guid, CurrencyPair currencyPair, KlineInterval klineInterval) : base(guid, currencyPair, klineInterval) { }
    public SimpleShortStrategyEngine(CurrencyPair currencyPair, KlineInterval klineInterval, decimal margin, decimal stopLossParameter, decimal takeProfitParameter, ICfdTradingService futuresTrader, ICfdMarketDataProvider futuresDataProvider, IFuturesMarketsCandlestickAwaiter candlestickAwaiter, IMediator mediator) : base(currencyPair, klineInterval, margin, stopLossParameter, takeProfitParameter, futuresTrader, futuresDataProvider, candlestickAwaiter, mediator)
    {
        if (this.StopLossParameter is <= 1)
            throw new ArgumentException($"When trading shorts the {nameof(this.StopLossParameter)} must be greater than 1", nameof(this.StopLossParameter));

        if (this.TakeProfitParameter is >= 1 or <= 0)
            throw new ArgumentException($"When trading shorts the {nameof(this.TakeProfitParameter)} must be between 0 and 1", nameof(this.TakeProfitParameter));
    }

    //// //// ////
    
    internal override async Task MakeMoveAsync()
    {
        if (this.Signal is null)
            return;
        
        if (this.Signal == TradingviewSignal.Down && !this.FuturesTrader.IsInPosition())
        {
            await this.OpenShortPositionAsync();
        }
        else if (this.Signal == TradingviewSignal.Up && this.FuturesTrader.IsInPosition())
        {
            await this.ClosePositionAsync();
        }

        this.Signal = null;
    }
    private async Task OpenShortPositionAsync()
    {
        var price = await this.FuturesTrader.GetCurrentPriceAsync();
        await this.FuturesTrader.OpenPositionAtMarketPriceAsync(OrderSide.Sell, this.Margin, this.StopLossParameter * price, this.TakeProfitParameter * price);

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
