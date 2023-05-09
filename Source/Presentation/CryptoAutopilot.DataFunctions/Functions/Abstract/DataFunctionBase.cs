using Application.Interfaces.Logging;
using Application.Interfaces.Services.DataAccess;

namespace CryptoAutopilot.DataFunctions.Functions.Abstract;

public abstract class MarketDataFunctionBase<T> where T : class
{
    protected readonly IFuturesTradesDBService DbService;
    protected readonly ILoggerAdapter<T> Logger;

    protected MarketDataFunctionBase(IFuturesTradesDBService dbService, ILoggerAdapter<T> logger)
    {
        this.DbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
        this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
