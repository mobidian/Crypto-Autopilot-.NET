using CryptoAutopilot.Api.Endpoints.Data.Market;

namespace CryptoAutopilot.Api.Endpoints;

public static class EndpointsExtensions
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapMarketDataEndpoints();

        return app;
    }
}
