namespace CryptoAutopilot.Api.Endpoints.Strategies;

public static class StrategiesEndpointsExtensions
{
    public static IEndpointRouteBuilder MapStrategiesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGetStrategyEndpoint();
        app.MapGetStrategiesEndpoint();

        app.MapStopStrategyEndpoint();

        return app;
    }
}
