using Application.Interfaces.Services.Trading;

using Binance.Net.Enums;

using Domain.Models;

using Infrastructure.Services.Trading;
using Infrastructure.Strategies.Abstract;

using MediatR;

namespace Infrastructure.Strategies.SimpleStrategy;

public abstract class SimpleStrategyEngine : StrategyEngine
{
    protected SimpleStrategyEngine(CurrencyPair currencyPair, KlineInterval klineInterval, ICfdTradingService futuresTrader, ICfdMarketDataProvider futuresDataProvider, IFuturesMarketsCandlestickAwaiter candlestickAwaiter, IMediator mediator) : base(currencyPair, klineInterval, futuresTrader, futuresDataProvider, candlestickAwaiter, mediator) { }

    
    protected internal enum TradingviewSignal { Up, Down }
    protected internal TradingviewSignal? Signal = null; // a null value indicates that the signal does not exist or has been consumed


    public void CFDMovingUp() => Signal = TradingviewSignal.Up;
    public void CFDMovingDown() => Signal = TradingviewSignal.Down;
}
