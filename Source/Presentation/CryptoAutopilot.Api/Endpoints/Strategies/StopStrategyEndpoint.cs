using CryptoAutopilot.Api.Endpoints.Strategies.Extensions;
using CryptoAutopilot.Api.Services.Interfaces;

using Microsoft.AspNetCore.Mvc;

namespace CryptoAutopilot.Api.Endpoints.Strategies;

public static class StopStrategyEndpoint
{
    public static IEndpointRouteBuilder MapStopStrategyEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete($"StopStrategy/{{guid}}", async ([FromServices] IStrategiesTracker StrategiesTracker, Guid guid, IServiceProvider services) =>
        {
            var strategy = StrategiesTracker.Get(guid);
            if (strategy is null)
                return Results.NotFound();

            return await strategy.StopAsync(services, TimeSpan.FromSeconds(15));
        }).WithTags("Strategies");


        return app;
    }
}
