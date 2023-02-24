using Application.Interfaces.Logging;
using Application.Interfaces.Proxies;
using Application.Interfaces.Services.General;
using Application.Interfaces.Services.Trading;
using Application.Interfaces.Services.Trading.Strategy;

using Binance.Net.Clients;
using Binance.Net.Interfaces.Clients;
using Binance.Net.Interfaces.Clients.UsdFuturesApi;
using Binance.Net.Objects;

using CryptoAutopilot.Api.Contracts.Responses.Strategies;
using CryptoAutopilot.Api.Endpoints.Internal;
using CryptoAutopilot.Api.Factories;
using CryptoAutopilot.Api.Services;
using CryptoAutopilot.Api.Services.Interfaces;

using Infrastructure;
using Infrastructure.Database.Contexts;
using Infrastructure.Logging;
using Infrastructure.Services.General;
using Infrastructure.Services.Proxies;
using Infrastructure.Services.Trading;

using Microsoft.AspNetCore.Mvc;

namespace CryptoAutopilot.Api.Endpoints;

public static class GeneralEndpoints
{
    public static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>));
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IInfrastructureMarker>());

        services.AddTransient(services => new FuturesTradingDbContext(configuration.GetConnectionString("CryptoPilotTrades")!, services.GetRequiredService<IDateTimeProvider>()));
        services.AddTransient<IFuturesTradesDBService, FuturesTradesDBService>();


        services.AddSingleton<ICfdMarketDataProvider, BinanceCfdMarketDataProvider>();
        services.AddSingleton<IUpdateSubscriptionProxy, UpdateSubscriptionProxy>();

        AddBinanceClientsAndServicesDerivedFromThem(services, configuration);

        // factories are used here because theese services need to be created
        // with respect to parameters such as currencyPair, timeframe, leverage and so on
        AddServiceFactories(services);

        services.AddSingleton<IStrategiesTracker, StrategiesTracker>();
    }
    private static void AddBinanceClientsAndServicesDerivedFromThem(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient(_ => new BinanceApiCredentials(configuration.GetValue<string>("BinanceApiCredentials:key")!, configuration.GetValue<string>("BinanceApiCredentials:secret")!));

        // binance client
        services.AddTransient<IBinanceClient, BinanceClient>(services =>
        {
            var client = new BinanceClient();
            client.SetApiCredentials(services.GetRequiredService<BinanceApiCredentials>());
            return client;
        });
        services.AddTransient(services => services.GetRequiredService<IBinanceClient>().UsdFuturesApi);
        services.AddTransient(services => services.GetRequiredService<IBinanceClientUsdFuturesApi>().Trading);
        services.AddTransient(services => services.GetRequiredService<IBinanceClientUsdFuturesApi>().ExchangeData);

        // binance socket client
        services.AddTransient<IBinanceSocketClient, BinanceSocketClient>(services =>
        {
            var socketClient = new BinanceSocketClient();
            socketClient.SetApiCredentials(services.GetRequiredService<BinanceApiCredentials>());
            return socketClient;
        });
        services.AddTransient(services => services.GetRequiredService<IBinanceSocketClient>().UsdFuturesStreams);
    }
    private static void AddServiceFactories(IServiceCollection services)
    {
        services.AddSingleton<ICfdTradingServiceFactory>();
        services.AddSingleton<IFuturesMarketsCandlestickAwaiterFactory>();
    }

    public static void MapEndpoints(this IEndpointRouteBuilder app)
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

        app.MapDelete($"StopStrategy/{{guid}}", async ([FromServices] IStrategiesTracker StrategiesTracker, Guid guid, IServiceProvider services) =>
        {
            var strategy = StrategiesTracker.Get(guid);
            if (strategy is null)
                return Results.NotFound();

            return await strategy.TryAwaitShutdownAsync(services, TimeSpan.FromSeconds(15));
        }).WithTags("Strategies");
    }
    private static GetStrategyEngineResponse StrategyEngineToResponse(IStrategyEngine strategy) => new GetStrategyEngineResponse
    {
        Guid = strategy.Guid,
        StartedStrategyTypeName = strategy.GetType().Name,
        IsRunning = strategy.IsRunning(),
    };
}
