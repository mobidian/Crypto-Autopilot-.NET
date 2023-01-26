using Application.Interfaces.Logging;
using Application.Interfaces.Proxies;
using Application.Interfaces.Services.General;
using Application.Interfaces.Services.Trading;

using Binance.Net.Clients;
using Binance.Net.Interfaces.Clients;
using Binance.Net.Interfaces.Clients.UsdFuturesApi;

using CryptoExchange.Net.Authentication;

using Infrastructure;
using Infrastructure.Database.Contexts;
using Infrastructure.Logging;
using Infrastructure.Services.General;
using Infrastructure.Services.Proxies;
using Infrastructure.Services.Trading;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using Presentation.Api.Contracts.Responses;
using Presentation.Api.Factories;

namespace Presentation.Api.Endpoints;

public static class GeneralEndpoints
{
    public static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>));
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        
        services.AddMediatR(typeof(IInfrastructureMarker).Assembly);

        services.AddTransient<FuturesTradingDbContext>(services => new FuturesTradingDbContext(configuration.GetConnectionString("CryptoPilotTrades")!, services.GetRequiredService<IDateTimeProvider>()));
        services.AddTransient<IFuturesTradesDBService, FuturesTradesDBService>();


        services.AddSingleton<ICfdMarketDataProvider, BinanceCfdMarketDataProvider>();
        services.AddSingleton<IUpdateSubscriptionProxy, UpdateSubscriptionProxy>();
        
        AddBinanceClientsAndServicesDerivedFromThem(services, configuration);

        AddServiceFactories(services);
    }
    private static void AddBinanceClientsAndServicesDerivedFromThem(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<ApiCredentials>(_ => new ApiCredentials(configuration.GetValue<string>("BinanceApiCredentials:key")!, configuration.GetValue<string>("BinanceApiCredentials:secret")!));
        
        // binance client
        services.AddTransient<IBinanceClient, BinanceClient>(services =>
        {
            var client = new BinanceClient();
            client.SetApiCredentials(services.GetRequiredService<ApiCredentials>());
            return client;
        });
        services.AddTransient<IBinanceClientUsdFuturesApi>(services => services.GetRequiredService<IBinanceClient>().UsdFuturesApi);
        services.AddTransient<IBinanceClientUsdFuturesApiTrading>(services => services.GetRequiredService<IBinanceClientUsdFuturesApi>().Trading);
        services.AddTransient<IBinanceClientUsdFuturesApiExchangeData>(services => services.GetRequiredService<IBinanceClientUsdFuturesApi>().ExchangeData);

        // binance socket client
        services.AddTransient<IBinanceSocketClient, BinanceSocketClient>(services =>
        {
            var socketClient = new BinanceSocketClient();
            socketClient.SetApiCredentials(services.GetRequiredService<ApiCredentials>());
            return socketClient;
        });
        services.AddTransient<IBinanceSocketClientUsdFuturesStreams>(services => services.GetRequiredService<IBinanceSocketClient>().UsdFuturesStreams);
    }
    private static void AddServiceFactories(IServiceCollection services)
    {
        // factories are used here because theese services need to be created
        // with respect to parameters such as currencyPair, timeframe, leverage and so on
        services.AddSingleton<ICfdTradingServiceFactory>();
        services.AddSingleton<IFuturesMarketsCandlestickAwaiterFactory>();
    }

    public static void MapEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("candlesticks", async ([FromServices] IFuturesTradesDBService DBService, [FromQuery] string? currencyPair) =>
        {
            if (currencyPair is null)
            {
                var candlesticks = await DBService.GetAllCandlesticksAsync();
                var response = new GetAllCandlesticksResponse { Candlesticks = candlesticks };
                return Results.Ok(response);
            }
            else
            {
                var candlesticks = await DBService.GetCandlesticksByCurrencyPairAsync(currencyPair);
                var response = new GetCandlesticksByCurrencyPairResponse
                {
                    CurrencyPair = currencyPair.ToUpper(),
                    Candlesticks = candlesticks,
                };
                return Results.Ok(response);
            }
        });
        
        app.MapGet("futuresorders", async ([FromServices] IFuturesTradesDBService DBService, [FromQuery] string? currencyPair) =>
        {
            if (currencyPair is null)
            {
                var futuresOrders = await DBService.GetAllFuturesOrdersAsync();
                var response = new GetAllFuturesOrdersResponse { FuturesOrders = futuresOrders };
                return Results.Ok(response);
            }
            else
            {
                var futuresOrders = await DBService.GetFuturesOrdersByCurrencyPairAsync(currencyPair);
                var response = new GetFuturesOrdersByCurrencyPairResponse
                {
                    CurrencyPair = currencyPair.ToUpper(),
                    FuturesOrders = futuresOrders,
                };
                return Results.Ok(response);
            }
        });
    }
}
