using System.Diagnostics;

using Application.Interfaces.Services.General;
using Application.Interfaces.Services.Trading.Strategy;

using Infrastructure.Strategies.SimpleStrategy;

using Microsoft.AspNetCore.Mvc;

using Presentation.Api.Contracts.Responses;
using Presentation.Api.Endpoints.Internal;

namespace Presentation.Api.Endpoints;

public class SimpleStrategyEndpoints : IEndpoints
{
    public static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<SimpleLongStrategyEngine>();
    }
    
    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("StartSimpleStrategy", ([FromServices] SimpleLongStrategyEngine engine, IServiceProvider services) =>
        {
            Task.Run(engine.StartTradingAsync);
            
            if (!WaitForStrategyToStartRunning(engine))
                return Results.Problem(detail: "The operation of starting the trading strategy engine has timed out after 10 seconds", type: "TimeoutException");
            
            return Results.Ok(new StrategyStartedResponse
            {
                Guid = engine.Guid,
                StrategyTypeName = engine.GetType().Name,
                Timestamp = services.GetRequiredService<IDateTimeProvider>().Now
            });
        });
        
        // // TODO app.MapPost("StopSimpleStrategy, () => { ... }");

        app.MapPost("CfdUp", ([FromServices] SimpleLongStrategyEngine engine) =>
        {
            engine.CFDMovingUp();
            return Results.Ok();
        });

        app.MapPost("CfdDown", ([FromServices] SimpleLongStrategyEngine engine) =>
        {
            engine.CFDMovingDown();
            return Results.Ok();
        });
    }
    private static bool WaitForStrategyToStartRunning(IStrategyEngine engine)
    {
        var stopwatch = Stopwatch.StartNew();
        
        while (!engine.IsRunning() && stopwatch.Elapsed.TotalSeconds < 10)
        {
            Thread.Sleep(50);
        }

        return engine.IsRunning();
    }
}
