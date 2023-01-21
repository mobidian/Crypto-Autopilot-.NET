using System.Diagnostics;

using Application.Interfaces.Services.General;
using Application.Interfaces.Services.Trading.Strategy;

using Presentation.Api.Contracts.Responses;

namespace Presentation.Api.Endpoints.Internal;

internal static class IStrategyEngineExtensions
{
    internal static async Task<IResult> LOL(this IStrategyEngine engine, IServiceProvider services, TimeSpan timeout)
    {
        _ = Task.Run(engine.StartTradingAsync);

        if (await engine.AwaitStartupAsync(timeout) == false)
            return Results.Problem(detail: $"The operation of starting the trading strategy engine has timed out after {timeout.Seconds} seconds", type: "TimeoutException");

        return Results.Ok(new StrategyStartedResponse
        {
            Guid = engine.Guid,
            StrategyTypeName = engine.GetType().Name,
            Timestamp = services.GetRequiredService<IDateTimeProvider>().Now
        });
    }
    internal static async Task<bool> AwaitStartupAsync(this IStrategyEngine engine, TimeSpan timeout)
    {
        var stopwatch = Stopwatch.StartNew();

        while (!engine.IsRunning() && stopwatch.Elapsed < timeout)
            await Task.Delay(50);
        
        return engine.IsRunning();
    }
}
