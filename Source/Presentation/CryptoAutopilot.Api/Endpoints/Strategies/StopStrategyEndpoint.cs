using CryptoAutopilot.Api.Endpoints.Strategies.Extensions;
using CryptoAutopilot.Api.Services.Interfaces;

namespace CryptoAutopilot.Api.Endpoints.Strategies;

public static class StopStrategyEndpoint
{
    public static IEndpointRouteBuilder MapStopStrategyEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete(ApiEndpoints.Strategies.Stop, async (Guid id, IStrategiesTracker StrategiesTracker, IServiceProvider services) =>
        {
            var strategy = StrategiesTracker.Get(id);
            if (strategy is null)
                return Results.NotFound();

            return await strategy.StopAsync(services, TimeSpan.FromSeconds(15));
        }).WithTags("Strategies");


        return app;
    }
}
