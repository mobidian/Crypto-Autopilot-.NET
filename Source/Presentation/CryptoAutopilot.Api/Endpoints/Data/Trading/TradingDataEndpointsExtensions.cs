using CryptoAutopilot.Api.Endpoints.Data.Market;

namespace CryptoAutopilot.Api.Endpoints.Data.Trading;

public static class TradingDataEndpointsExtensions
{
    public static IEndpointRouteBuilder MapTradingDataEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGetFuturesOrdersEndpoint();
        app.MapGetFuturesPositionsEndpoint();

        return app;
    }
}
