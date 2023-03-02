using Application.Interfaces.Logging;
using Application.Interfaces.Proxies;
using Application.Interfaces.Services.General;
using Application.Interfaces.Services.Trading;
using Application.Interfaces.Services.Trading.Monitors;

using Binance.Net.Clients;
using Binance.Net.Interfaces.Clients;
using Binance.Net.Interfaces.Clients.UsdFuturesApi;
using Binance.Net.Objects;

using CryptoAutopilot.Api.Factories;
using CryptoAutopilot.Api.Services;
using CryptoAutopilot.Api.Services.Interfaces;

using Infrastructure;
using Infrastructure.Database.Contexts;
using Infrastructure.Logging;
using Infrastructure.Services.General;
using Infrastructure.Services.Proxies;
using Infrastructure.Services.Trading;
using Infrastructure.Services.Trading.Monitors;

namespace CryptoAutopilot.Api.Endpoints;

public static partial class ServicesEndpointsExtensions
{
    public static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>));
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IInfrastructureMarker>());

        services.AddTransient(services => new FuturesTradingDbContext(configuration.GetConnectionString("OrderHistoryDB")!, services.GetRequiredService<IDateTimeProvider>()));
        services.AddTransient<IFuturesTradesDBService, FuturesTradesDBService>();


        services.AddSingleton<IFuturesMarketDataProvider, BinanceFuturesMarketDataProvider>();
        services.AddSingleton<IUpdateSubscriptionProxy, UpdateSubscriptionProxy>();

        AddBinanceClientsAndServicesDerivedFromThem(services, configuration);

        services.AddSingleton<IBinanceFuturesApiService, BinanceFuturesApiService>();

        services.AddSingleton<IOrderStatusMonitor, OrderStatusMonitor>();
        services.AddSingleton<IFuturesCandlesticksMonitor, FuturesCandlesticksMonitor>();
        
        services.AddSingleton<IBinanceFuturesAccountDataProvider, BinanceFuturesAccountDataProvider>(services => new BinanceFuturesAccountDataProvider(services.GetRequiredService<IBinanceClientUsdFuturesApiAccount>()));

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
        services.AddTransient<IBinanceClientUsdFuturesApi>(services => services.GetRequiredService<IBinanceClient>().UsdFuturesApi);
        services.AddTransient<IBinanceClientUsdFuturesApiTrading>(services => services.GetRequiredService<IBinanceClientUsdFuturesApi>().Trading);
        services.AddTransient<IBinanceClientUsdFuturesApiExchangeData>(services => services.GetRequiredService<IBinanceClientUsdFuturesApi>().ExchangeData);
        services.AddTransient<IBinanceClientUsdFuturesApiAccount>(services => services.GetRequiredService<IBinanceClientUsdFuturesApi>().Account);
        
        // binance socket client
        services.AddTransient<IBinanceSocketClient, BinanceSocketClient>(services =>
        {
            var socketClient = new BinanceSocketClient();
            socketClient.SetApiCredentials(services.GetRequiredService<BinanceApiCredentials>());
            return socketClient;
        });
        services.AddTransient<IBinanceSocketClientUsdFuturesStreams>(services => services.GetRequiredService<IBinanceSocketClient>().UsdFuturesStreams);
    }
    private static void AddServiceFactories(IServiceCollection services)
    {
        services.AddSingleton<FuturesTradingServiceFactory>();
        services.AddSingleton<Func<IUpdateSubscriptionProxy>>(services => () => services.GetRequiredService<IUpdateSubscriptionProxy>());
    }
}
