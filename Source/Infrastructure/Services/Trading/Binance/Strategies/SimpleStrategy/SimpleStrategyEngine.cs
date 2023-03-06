using Application.Interfaces.Services.Trading.Binance;
using Application.Interfaces.Services.Trading.Binance.Monitors;

using Binance.Net.Enums;

using Domain.Models;

using Infrastructure.Services.Trading.Binance.Strategies.Abstract;
using Infrastructure.Services.Trading.Binance.Strategies.SimpleStrategy.Enums;

using MediatR;

namespace Infrastructure.Services.Trading.Binance.Strategies.SimpleStrategy;

public abstract class SimpleStrategyEngine : StrategyEngine
{
    protected readonly decimal Margin;
    protected readonly decimal StopLossParameter;
    protected readonly decimal TakeProfitParameter;

    internal SimpleStrategyEngine(Guid guid, CurrencyPair currencyPair, KlineInterval klineInterval) : base(guid, currencyPair, klineInterval) { }
    protected SimpleStrategyEngine(CurrencyPair currencyPair, KlineInterval klineInterval, decimal margin, decimal stopLossParameter, decimal takeProfitParameter, IFuturesTradingService futuresTrader, IFuturesMarketDataProvider futuresDataProvider, IFuturesCandlesticksMonitor candlestickMonitor, IMediator mediator) : base(currencyPair, klineInterval, futuresTrader, futuresDataProvider, candlestickMonitor, mediator)
    {
        this.Margin = margin;
        this.StopLossParameter = stopLossParameter;
        this.TakeProfitParameter = takeProfitParameter;
    }


    protected internal TradingviewSignal? Signal = null; // a null value indicates that the signal does not exist or has been consumed

    public void CFDMovingUp() => this.Signal = TradingviewSignal.Up;
    public void CFDMovingDown() => this.Signal = TradingviewSignal.Down;
}
