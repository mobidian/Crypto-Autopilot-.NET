using System.Diagnostics;

using Application.Interfaces.Services.General;
using Application.Strategies;

using CryptoAutopilot.Api.Services.Interfaces;
using CryptoAutopilot.Contracts.Responses.Strategies;

namespace CryptoAutopilot.Api.Endpoints.Internal;

internal static class IStrategyEngineExtensions
{
    internal static async Task<IResult> StartAsync(this IStrategyEngine engine, IServiceProvider services, TimeSpan timeout)
    {
        _ = Task.Run(engine.StartTradingAsync);

        await engine.WaitForRunningTrueAsync(timeout);
        if (!engine.IsRunning())
            return Results.Problem(detail: $"The operation of starting the trading strategy engine has timed out after {timeout.Seconds} seconds", type: "TimeoutException");

        services.GetRequiredService<IStrategiesTracker>().Add(engine);
        return Results.Ok(new StrategyEngineStartedResponse
        {
            Guid = engine.Guid,
            StartedStrategyTypeName = engine.GetType().Name,
            StartTime = services.GetRequiredService<IDateTimeProvider>().Now
        });
    }
    internal static async Task WaitForRunningTrueAsync(this IStrategyEngine engine, TimeSpan timeout)
    {
        var timestamp = Stopwatch.GetTimestamp();

        while (!engine.IsRunning() && Stopwatch.GetElapsedTime(timestamp) < timeout)
            await Task.Delay(50);
    }

    internal static async Task<IResult> StopAsync(this IStrategyEngine engine, IServiceProvider services, TimeSpan timeout)
    {
        _ = Task.Run(engine.StopTradingAsync);

        await engine.WaitForRunningFalseAsync(timeout);
        if (engine.IsRunning())
            return Results.Problem(detail: $"The operation of stopping the trading strategy engine has timed out after {timeout.Seconds} seconds", type: "TimeoutException");

        return Results.Ok(new StrategyEngineStoppedResponse
        {
            Guid = engine.Guid,
            StoppedStrategyTypeName = engine.GetType().Name,
            StopTime = services.GetRequiredService<IDateTimeProvider>().Now
        });
    }
    internal static async Task WaitForRunningFalseAsync(this IStrategyEngine engine, TimeSpan timeout)
    {
        var timestamp = Stopwatch.GetTimestamp();

        while (engine.IsRunning() && Stopwatch.GetElapsedTime(timestamp) < timeout)
            await Task.Delay(50);
    }
}
