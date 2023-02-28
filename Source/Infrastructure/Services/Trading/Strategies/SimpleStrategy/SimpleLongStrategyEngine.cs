using Application.Interfaces.Services.Trading;
using Application.Interfaces.Services.Trading.Monitors;

using Binance.Net.Enums;

using Domain.Models;

using Infrastructure.Notifications;
using Infrastructure.Services.Trading.Strategies.SimpleStrategy.Enums;

using MediatR;

namespace Infrastructure.Services.Trading.Strategies.SimpleStrategy;

/// <summary>
/// A simple strategy that opens a long position when <see cref="CFDMovingUp"/> is called and closes the position if it exists when <see cref="CFDMovingDown"/> is called
/// </summary>
public sealed class SimpleLongStrategyEngine : SimpleStrategyEngine
{
    internal SimpleLongStrategyEngine(Guid guid, CurrencyPair currencyPair, KlineInterval klineInterval) : base(guid, currencyPair, klineInterval) { }
    public SimpleLongStrategyEngine(CurrencyPair currencyPair, KlineInterval klineInterval, decimal margin, decimal stopLossParameter, decimal takeProfitParameter, ICfdTradingService futuresTrader, ICfdMarketDataProvider futuresDataProvider, IFuturesCandlesticksMonitor candlestickMonitor, IMediator mediator) : base(currencyPair, klineInterval, margin, stopLossParameter, takeProfitParameter, futuresTrader, futuresDataProvider, candlestickMonitor, mediator)
    {
        if (this.StopLossParameter is >= 1 or <= 0)
            throw new ArgumentException($"When trading longs the {nameof(this.StopLossParameter)} must be between 0 and 1", nameof(this.StopLossParameter));

        if (this.TakeProfitParameter <= 1)
            throw new ArgumentException($"When trading longs the {nameof(this.TakeProfitParameter)} must be greater than 1", nameof(this.TakeProfitParameter));
    }

    //// //// ////

    internal override async Task MakeMoveAsync()
    {
        await this.CandlestickMonitor.WaitForNextCandlestickAsync(this.CurrencyPair.Name, ContractType.Perpetual, this.KlineInterval);


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
        var price = await this.FuturesDataProvider.GetCurrentPriceAsync(this.CurrencyPair.Name);
        await this.FuturesTrader.PlaceMarketOrderAsync(OrderSide.Buy, this.Margin, this.StopLossParameter * price, this.TakeProfitParameter * price);

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
