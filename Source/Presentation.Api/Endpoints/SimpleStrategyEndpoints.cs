using Infrastructure.Strategies.SimpleStrategy;

using Microsoft.AspNetCore.Mvc;

using Presentation.Api.Endpoints.Internal;

namespace Presentation.Api.Endpoints;

public class SimpleStrategyEndpoints : IEndpoints
{
    public static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<SimpleStrategyEngine>();
    }
    
    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("CfdUp", ([FromServices] SimpleStrategyEngine engine) =>
        {
            engine.CFDMovingUp();
            return Results.Ok();
        });

        app.MapPost("CfdDown", ([FromServices] SimpleStrategyEngine engine) =>
        {
            engine.CFDMovingDown();
            return Results.Ok();
        });
    }
}
