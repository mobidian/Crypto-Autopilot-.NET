using Application.Interfaces.Logging;
using Application.Interfaces.Services;

namespace CryptoAutopilot.DataFunctions.Functions;

public abstract class DataFunctionBase
{
    protected readonly IFuturesTradesDBService DbService;
    protected readonly ILoggerAdapter<GetCandlesticksFunction> Logger;

    protected DataFunctionBase(IFuturesTradesDBService dbService, ILoggerAdapter<GetCandlesticksFunction> logger)
    {
        this.DbService = dbService;
        this.Logger = logger;
    }
}
