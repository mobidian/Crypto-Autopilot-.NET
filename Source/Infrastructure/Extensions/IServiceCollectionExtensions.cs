using Application.Interfaces.Logging;
using Application.Interfaces.Proxies;
using Application.Interfaces.Services.Bybit;
using Application.Interfaces.Services.Bybit.Monitors;
using Application.Interfaces.Services.DataAccess.Repositories;
using Application.Interfaces.Services.DataAccess.Services;
using Application.Interfaces.Services.General;

using Bybit.Net.Clients;
using Bybit.Net.Interfaces.Clients;
using Bybit.Net.Interfaces.Clients.UsdPerpetualApi;

using CryptoExchange.Net.Authentication;

using Infrastructure.Database;
using Infrastructure.Factories;
using Infrastructure.Logging;
using Infrastructure.Proxies;
using Infrastructure.Services.Bybit;
using Infrastructure.Services.Bybit.Monitors;
using Infrastructure.Services.DataAccess.Repositories;
using Infrastructure.Services.DataAccess.Services;
using Infrastructure.Services.General;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class IServiceCollectionExtensions
{
    public static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>));
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IInfrastructureMarker>());

        services.AddDbContext<FuturesTradingDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("TradingHistoryDB")!));
        services.AddScoped<IFuturesOrdersRepository, FuturesOrdersRepository>();
        services.AddScoped<IFuturesPositionsRepository, FuturesPositionsRepository>();
        services.AddDataAccessServices();

        services.AddSingleton<IUpdateSubscriptionProxy, UpdateSubscriptionProxy>();
        services.AddSingleton<Func<IUpdateSubscriptionProxy>>(services => () => services.GetRequiredService<IUpdateSubscriptionProxy>());
        
        services.AddBybitServices(configuration);
        services.AddBybitServiceFactories();
    }
    
    private static void AddDataAccessServices(this IServiceCollection services)
    {
        services.AddScoped<IFuturesOperationsService, FuturesOperationsService>(services =>
        {
            var dbContext = services.GetRequiredService<FuturesTradingDbContext>();
            return new FuturesOperationsService(dbContext, new FuturesPositionsRepository(dbContext), new FuturesOrdersRepository(dbContext));
        });
    }
    private static void AddBybitServices(this IServiceCollection services, IConfiguration configuration)
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
    private static void AddBybitServiceFactories(this IServiceCollection services)
    {
        services.AddSingleton<BybitUsdFuturesTradingServiceFactory>();
    }
}
