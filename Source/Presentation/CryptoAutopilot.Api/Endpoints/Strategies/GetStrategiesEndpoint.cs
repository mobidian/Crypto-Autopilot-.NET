using Application.Strategies;

using CryptoAutopilot.Api.Services.Interfaces;
using CryptoAutopilot.Contracts.Responses.Strategies;

using Microsoft.AspNetCore.Mvc;

namespace CryptoAutopilot.Api.Endpoints.Strategies;

public static class GetStrategiesEndpoint
{
    public static IEndpointRouteBuilder MapGetStrategiesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("strategies", ([FromServices] IStrategiesTracker StrategiesTracker, Guid? guid, IServiceProvider services) =>
        {
            if (guid is null)
            {
                var strategies = StrategiesTracker.GetAll();
                var responses = strategies.Select(StrategyEngineToResponse);
                var response = new GetAllStrategyEnginesResponse { Strategies = responses };
                return Results.Ok(response);
            }
            else
            {
                var strategy = StrategiesTracker.Get(guid.Value);
                if (strategy is null)
                    return Results.NotFound();

                var response = StrategyEngineToResponse(strategy);
                return Results.Ok(response);
            }
        }).WithTags("Strategies");


        return app;
    }
    private static StrategyEngineResponse StrategyEngineToResponse(IStrategyEngine strategy) => new StrategyEngineResponse
    {
        Guid = strategy.Guid,
        StartedStrategyTypeName = strategy.GetType().Name,
        IsRunning = strategy.IsRunning(),
    };
}
