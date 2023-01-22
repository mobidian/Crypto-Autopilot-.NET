using Infrastructure.Strategies.SimpleStrategy;

using Microsoft.AspNetCore.Mvc;

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
        MapStartStopEndpoints(app);
        MapStrategySignalsEndpoints(app);
    }
    private static void MapStartStopEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("StartSimpleStrategy", async ([FromServices] SimpleLongStrategyEngine engine, IServiceProvider services) => await engine.TryAwaitStartupAsync(services, TimeSpan.FromSeconds(15)));

        app.MapPost("StopSimpleStrategy", async ([FromServices] SimpleLongStrategyEngine engine, IServiceProvider services) => await engine.TryAwaitShutdownAsync(services, TimeSpan.FromSeconds(15)));
    }
    private static void MapStrategySignalsEndpoints(IEndpointRouteBuilder app)
    {
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
}
