using Infrastructure.Strategies.SimpleStrategy;

using Microsoft.AspNetCore.Mvc;

using Presentation.Api.Endpoints.Internal;

namespace Presentation.Api.Endpoints;

public class SimpleShortStrategyEndpoints : IEndpoints
{
    public static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<SimpleShortStrategyEngine>();
    }
    
    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        MapStartStopEndpoints(app);
        MapStrategySignalsEndpoints(app);
    }
    private static void MapStartStopEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("StartSimpleShortStrategy", async ([FromServices] SimpleShortStrategyEngine engine, IServiceProvider services) => await engine.TryAwaitStartupAsync(services, TimeSpan.FromSeconds(15)));

        app.MapPost("StopSimpleShortStrategy", async ([FromServices] SimpleShortStrategyEngine engine, IServiceProvider services) => await engine.TryAwaitShutdownAsync(services, TimeSpan.FromSeconds(15)));
    }
    private static void MapStrategySignalsEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("SimpleShortStrategyCfdUp", ([FromServices] SimpleShortStrategyEngine engine) =>
        {
            engine.CFDMovingUp();
            return Results.Ok();
        });

        app.MapPost("SimpleShortStrategyCfdDown", ([FromServices] SimpleShortStrategyEngine engine) =>
        {
            engine.CFDMovingDown();
            return Results.Ok();
        });
    }
}
