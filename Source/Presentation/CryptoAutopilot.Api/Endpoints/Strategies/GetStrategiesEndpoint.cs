using CryptoAutopilot.Api.Services.Interfaces;
using CryptoAutopilot.Contracts.Responses.Strategies;

namespace CryptoAutopilot.Api.Endpoints.Strategies;

public static class GetStrategiesEndpoint
{
    public static IEndpointRouteBuilder MapGetStrategiesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Strategies.GetAll, (IStrategiesTracker StrategiesTracker, IServiceProvider services) =>
        {
            var strategies = StrategiesTracker.GetAll();
            var responses = strategies.Select(x => new StrategyEngineResponse
            {
                Guid = x.Guid,
                StartedStrategyTypeName = x.GetType().Name,
                IsRunning = x.IsRunning(),
            });
            var response = new GetAllStrategyEnginesResponse { Strategies = responses };
            return Results.Ok(response);
        })
        .WithTags("Strategies");


        return app;
    }
}
