using System.Diagnostics;

using Application.Interfaces.Services.General;
using Application.Interfaces.Services.Trading;

using CryptoAutopilot.Api.Contracts.Responses.Strategies;
using CryptoAutopilot.Api.Services.Interfaces;

namespace CryptoAutopilot.Api.Endpoints.Internal;

internal static class IStrategyEngineExtensions
{
    internal static async Task<IResult> TryAwaitStartupAsync(this IStrategyEngine engine, IServiceProvider services, TimeSpan timeout)
    {
        _ = Task.Run(engine.StartTradingAsync);

        if (await engine.AwaitStartupAsync(timeout) == false)
            return Results.Problem(detail: $"The operation of starting the trading strategy engine has timed out after {timeout.Seconds} seconds", type: "TimeoutException");

        services.GetRequiredService<IStrategiesTracker>().Add(engine);
        return Results.Ok(new StrategyEngineStartedResponse
        {
            Guid = engine.Guid,
            StartedStrategyTypeName = engine.GetType().Name,
            StartTime = services.GetRequiredService<IDateTimeProvider>().Now
        });
    }
    internal static async Task<bool> AwaitStartupAsync(this IStrategyEngine engine, TimeSpan timeout)
    {
        var stopwatch = Stopwatch.StartNew();

        while (!engine.IsRunning() && stopwatch.Elapsed < timeout)
            await Task.Delay(50);

        return engine.IsRunning();
    }

    internal static async Task<IResult> TryAwaitShutdownAsync(this IStrategyEngine engine, IServiceProvider services, TimeSpan timeout)
    {
        _ = Task.Run(engine.StopTradingAsync);

        timeout += TimeSpan.FromSeconds((int)engine.KlineInterval);
        if (await engine.AwaitShutdownAsync(timeout) == false)
            return Results.Problem(detail: $"The operation of stopping the trading strategy engine has timed out after {timeout.Seconds} seconds", type: "TimeoutException");

        return Results.Ok(new StrategyEngineStoppedResponse
        {
            Guid = engine.Guid,
            StoppedStrategyTypeName = engine.GetType().Name,
            StopTime = services.GetRequiredService<IDateTimeProvider>().Now
        });
    }
    internal static async Task<bool> AwaitShutdownAsync(this IStrategyEngine engine, TimeSpan timeout)
    {
        var stopwatch = Stopwatch.StartNew();

        while (engine.IsRunning() && stopwatch.Elapsed < timeout)
            await Task.Delay(50);

        return !engine.IsRunning();
    }
}
