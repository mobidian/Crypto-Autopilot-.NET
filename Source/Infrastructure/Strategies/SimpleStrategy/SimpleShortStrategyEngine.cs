using Application.Interfaces.Services.Trading;
using Application.Interfaces.Services.Trading.Monitors;

using Binance.Net.Enums;

using Domain.Models;

using Infrastructure.Notifications;
using Infrastructure.Strategies.SimpleStrategy.Enums;

using MediatR;

namespace Infrastructure.Strategies.SimpleStrategy;

/// <summary>
/// A simple strategy that opens a short position when <see cref="CFDMovingDown"/> is called and closes the position if it exists when <see cref="CFDMovingUp"/> is called
/// </summary>
public sealed class SimpleShortStrategyEngine : SimpleStrategyEngine
{
    internal SimpleShortStrategyEngine(Guid guid, CurrencyPair currencyPair, KlineInterval klineInterval) : base(guid, currencyPair, klineInterval) { }
    public SimpleShortStrategyEngine(CurrencyPair currencyPair, KlineInterval klineInterval, decimal margin, decimal stopLossParameter, decimal takeProfitParameter, ICfdTradingService futuresTrader, ICfdMarketDataProvider futuresDataProvider, IFuturesCandlesticksMonitor candlestickMonitor, IMediator mediator) : base(currencyPair, klineInterval, margin, stopLossParameter, takeProfitParameter, futuresTrader, futuresDataProvider, candlestickMonitor, mediator)
    {
        if (this.StopLossParameter is <= 1)
            throw new ArgumentException($"When trading shorts the {nameof(this.StopLossParameter)} must be greater than 1", nameof(this.StopLossParameter));

        if (this.TakeProfitParameter is >= 1 or <= 0)
            throw new ArgumentException($"When trading shorts the {nameof(this.TakeProfitParameter)} must be between 0 and 1", nameof(this.TakeProfitParameter));
    }

    //// //// ////

    internal override async Task MakeMoveAsync()
    {
        await this.CandlestickMonitor.WaitForNextCandlestickAsync(this.CurrencyPair.Name, ContractType.Perpetual, this.KlineInterval);


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
        var price = await this.FuturesDataProvider.GetCurrentPriceAsync(this.CurrencyPair.Name);
        await this.FuturesTrader.PlaceMarketOrderAsync(OrderSide.Sell, this.Margin, this.StopLossParameter * price, this.TakeProfitParameter * price);

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
