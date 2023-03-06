using Application.Interfaces.Logging;
using Application.Interfaces.Proxies;
using Application.Interfaces.Services.General;
using Application.Interfaces.Services.Trading.Binance;
using Application.Interfaces.Services.Trading.Binance.Monitors;
using Application.Interfaces.Services.Trading.BybitExchange;
using Application.Interfaces.Services.Trading.BybitExchange.Monitors;

using Binance.Net.Clients;
using Binance.Net.Interfaces.Clients;
using Binance.Net.Interfaces.Clients.UsdFuturesApi;
using Binance.Net.Objects;

using Bybit.Net.Clients;
using Bybit.Net.Interfaces.Clients;
using Bybit.Net.Interfaces.Clients.UsdPerpetualApi;

using CryptoAutopilot.Api.Factories;
using CryptoAutopilot.Api.Services;
using CryptoAutopilot.Api.Services.Interfaces;

using CryptoExchange.Net.Authentication;

using Infrastructure;
using Infrastructure.Database.Contexts;
using Infrastructure.Logging;
using Infrastructure.Services.General;
using Infrastructure.Services.Proxies;
using Infrastructure.Services.Trading.Binance;
using Infrastructure.Services.Trading.Binance.Monitors;
using Infrastructure.Services.Trading.BybitExchange;
using Infrastructure.Services.Trading.BybitExchange.Monitors;

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

        services.AddSingleton<IUpdateSubscriptionProxy, UpdateSubscriptionProxy>();

        AddBinanceServices(services, configuration);
        AddBinanceServiceFactories(services);

        AddBybitServices(services, configuration);
        AddBybitServiceFactories(services);
        
        services.AddSingleton<IStrategiesTracker, StrategiesTracker>();
    }
    
    private static void AddBinanceServices(IServiceCollection services, IConfiguration configuration)
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


        services.AddSingleton<IFuturesMarketDataProvider, BinanceFuturesMarketDataProvider>();
        
        services.AddSingleton<IBinanceFuturesApiService, BinanceFuturesApiService>();

        services.AddSingleton<IOrderStatusMonitor, OrderStatusMonitor>();
        services.AddSingleton<IFuturesCandlesticksMonitor, FuturesCandlesticksMonitor>();

        services.AddSingleton<IBinanceFuturesAccountDataProvider, BinanceFuturesAccountDataProvider>(services => new BinanceFuturesAccountDataProvider(services.GetRequiredService<IBinanceClientUsdFuturesApiAccount>()));
    }
    private static void AddBinanceServiceFactories(IServiceCollection services)
    {
        services.AddSingleton<BinanceFuturesTradingServiceFactory>();
        services.AddSingleton<Func<IUpdateSubscriptionProxy>>(services => () => services.GetRequiredService<IUpdateSubscriptionProxy>());
    }

    private static void AddBybitServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ApiCredentials>(_ => new ApiCredentials(configuration.GetValue<string>("BybitApiCredentials:key")!, configuration.GetValue<string>("BybitApiCredentials:secret")!));

        // bybit client
        services.AddSingleton<IBybitClient, BybitClient>(services =>
        {
            var client = new BybitClient();
            client.SetApiCredentials(services.GetRequiredService<ApiCredentials>());
            return client;
        });
        services.AddSingleton<IBybitClientUsdPerpetualApi>(services => services.GetRequiredService<IBybitClient>().UsdPerpetualApi);
        services.AddSingleton<IBybitClientUsdPerpetualApiTrading>(services => services.GetRequiredService<IBybitClientUsdPerpetualApi>().Trading);
        services.AddSingleton<IBybitClientUsdPerpetualApiExchangeData>(services => services.GetRequiredService<IBybitClientUsdPerpetualApi>().ExchangeData);
        services.AddSingleton<IBybitClientUsdPerpetualApiAccount>(services => services.GetRequiredService<IBybitClientUsdPerpetualApi>().Account);
        
        // bybit socket client
        services.AddSingleton<IBybitSocketClient, BybitSocketClient>(services =>
        {
            var client = new BybitSocketClient();
            client.SetApiCredentials(services.GetRequiredService<ApiCredentials>());
            return client;
        });
        services.AddSingleton<IBybitSocketClientUsdPerpetualStreams>(services => services.GetRequiredService<IBybitSocketClient>().UsdPerpetualStreams);
        
        services.AddSingleton<IBybitFuturesAccountDataProvider, BybitFuturesAccountDataProvider>();
        services.AddSingleton<IBybitUsdFuturesMarketDataProvider, BybitUsdFuturesMarketDataProvider>();
        services.AddSingleton<IBybitUsdFuturesTradingApiClient, BybitUsdFuturesTradingApiClient>();
        services.AddSingleton<IBybitUsdPerpetualKlinesMonitor, BybitUsdPerpetualKlinesMonitor>();
    }
    private static void AddBybitServiceFactories(IServiceCollection services)
    {
        services.AddSingleton<BybitUsdFuturesTradingServiceFactory>();
    }
}
