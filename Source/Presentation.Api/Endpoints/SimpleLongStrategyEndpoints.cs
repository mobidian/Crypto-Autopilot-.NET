using Infrastructure.Strategies.SimpleStrategy;

using Microsoft.AspNetCore.Mvc;

using Presentation.Api.Endpoints.Internal;

namespace Presentation.Api.Endpoints;

public class SimpleLongStrategyEndpoints : IEndpoints
{
    public static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<SimpleLongStrategyEngine>();
    }

    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        MapStartStopEndpoints(app);
        MapStrategySignalsEndpoints(app);
    }
    private static void MapStartStopEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("StartSimpleLongStrategy", async ([FromServices] SimpleLongStrategyEngine engine, IServiceProvider services) => await engine.TryAwaitStartupAsync(services, TimeSpan.FromSeconds(15)));

        app.MapPost("StopSimpleLongStrategy", async ([FromServices] SimpleLongStrategyEngine engine, IServiceProvider services) => await engine.TryAwaitShutdownAsync(services, TimeSpan.FromSeconds(15)));
    }
    private static void MapStrategySignalsEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("SimpleLongStrategyCfdUp", ([FromServices] SimpleLongStrategyEngine engine) =>
        {
            engine.CFDMovingUp();
            return Results.Ok();
        });

        app.MapPost("SimpleLongStrategyCfdDown", ([FromServices] SimpleLongStrategyEngine engine) =>
        {
            engine.CFDMovingDown();
            return Results.Ok();
        });
    }
}
