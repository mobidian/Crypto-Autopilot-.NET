namespace CryptoAutopilot.Api.Endpoints.Data.Market;

public static class MarketDataEndpointsExtensions
{
    public static IEndpointRouteBuilder MapMarketDataEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGetKlinesEndpoint();

        return app;
    }
}
