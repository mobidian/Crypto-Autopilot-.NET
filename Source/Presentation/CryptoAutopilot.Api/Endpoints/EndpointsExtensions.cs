using CryptoAutopilot.Api.Endpoints.Data.Market;
using CryptoAutopilot.Api.Endpoints.Data.Trading;
using CryptoAutopilot.Api.Endpoints.Strategies;

namespace CryptoAutopilot.Api.Endpoints;

public static class EndpointsExtensions
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapMarketDataEndpoints();
        app.MapTradingDataEndpoints();

        app.MapStrategiesEndpoints();

        return app;
    }
}
