using Application.Interfaces.Logging;
using Application.Interfaces.Services;

namespace CryptoAutopilot.DataFunctions.Functions;

public abstract class DataFunctionBase<T> where T : class
{
    protected readonly IFuturesTradesDBService DbService;
    protected readonly ILoggerAdapter<T> Logger;

    protected DataFunctionBase(IFuturesTradesDBService dbService, ILoggerAdapter<T> logger)
    {
        this.DbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
        this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
