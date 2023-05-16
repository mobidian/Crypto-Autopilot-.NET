using Application.Interfaces.Logging;

namespace CryptoAutopilot.DataFunctions.Functions.Abstract;

public abstract class MarketDataFunctionBase<T> where T : class
{
    protected readonly ILoggerAdapter<T> Logger;

    protected MarketDataFunctionBase(ILoggerAdapter<T> logger)
    {
        this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
