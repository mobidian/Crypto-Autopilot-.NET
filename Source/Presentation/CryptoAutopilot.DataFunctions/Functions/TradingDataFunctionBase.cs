using Application.Interfaces.Logging;
using Application.Interfaces.Services.Bybit;

namespace CryptoAutopilot.DataFunctions.Functions;

public abstract class TradingDataFunctionBase<T> where T : class
{
    protected readonly IBybitUsdFuturesMarketDataProvider MarketDataProvider;
    protected readonly ILoggerAdapter<T> Logger;
    
    protected TradingDataFunctionBase(IBybitUsdFuturesMarketDataProvider marketDataProvider, ILoggerAdapter<T> logger)
    {
        this.MarketDataProvider = marketDataProvider ?? throw new ArgumentNullException(nameof(marketDataProvider));
        this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}