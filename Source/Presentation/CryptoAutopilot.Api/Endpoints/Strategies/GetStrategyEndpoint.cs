using CryptoAutopilot.Api.Services.Interfaces;
using CryptoAutopilot.Contracts.Responses.Strategies;

namespace CryptoAutopilot.Api.Endpoints.Strategies;

public static class GetStrategyEndpoint
{
    public static IEndpointRouteBuilder MapGetStrategyEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Strategies.Get, (Guid id, IStrategiesTracker StrategiesTracker, IServiceProvider services) =>
        {
            var strategy = StrategiesTracker.Get(id);
            if (strategy is null)
                return Results.NotFound();

            var response = new StrategyEngineResponse
            {
                Guid = strategy.Guid,
                StartedStrategyTypeName = strategy.GetType().Name,
                IsRunning = strategy.IsRunning(),
            };
            return Results.Ok(response);
        })
        .WithTags("Strategies");


        return app;
    }
}
