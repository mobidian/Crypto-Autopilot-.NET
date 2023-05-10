using Application.Interfaces.Logging;
using Application.Interfaces.Proxies;
using Application.Interfaces.Services.Bybit;
using Application.Interfaces.Services.Bybit.Monitors;
using Application.Interfaces.Services.DataAccess;
using Application.Interfaces.Services.General;

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
using Infrastructure.Proxies;
using Infrastructure.Services.Bybit;
using Infrastructure.Services.Bybit.Monitors;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.General;

using Microsoft.EntityFrameworkCore;

namespace CryptoAutopilot.Api.Endpoints;

public static partial class ServicesEndpointsExtensions
{
    public static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>));
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IInfrastructureMarker>());
        
        services.AddDbContext<FuturesTradingDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("TradingHistoryDB")!));
        services.AddScoped<IFuturesOrdersRepository, FuturesOrdersRepository>();
        services.AddScoped<IFuturesPositionsRepository, FuturesPositionsRepository>();

        services.AddSingleton<IUpdateSubscriptionProxy, UpdateSubscriptionProxy>();
        services.AddSingleton<Func<IUpdateSubscriptionProxy>>(services => () => services.GetRequiredService<IUpdateSubscriptionProxy>());

        AddBybitServices(services, configuration);
        AddBybitServiceFactories(services);
        
        services.AddSingleton<IStrategiesTracker, StrategiesTracker>();
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
