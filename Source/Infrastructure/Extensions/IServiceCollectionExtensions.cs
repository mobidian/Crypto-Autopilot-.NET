using Application;
using Application.DataAccess.Repositories;
using Application.DataAccess.Services;
using Application.Interfaces.Logging;
using Application.Interfaces.Proxies;
using Application.Interfaces.Services.Bybit;
using Application.Interfaces.Services.Bybit.Monitors;
using Application.Interfaces.Services.General;

using Bybit.Net.Clients;
using Bybit.Net.Interfaces.Clients;
using Bybit.Net.Interfaces.Clients.UsdPerpetualApi;
using Bybit.Net.Objects;

using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;

using Domain;
using Domain.PipelineBehaviors;

using FluentValidation;

using Infrastructure.DataAccess.Database;
using Infrastructure.DataAccess.Repositories;
using Infrastructure.DataAccess.Services;
using Infrastructure.Factories;
using Infrastructure.Logging;
using Infrastructure.Proxies;
using Infrastructure.Services.Bybit;
using Infrastructure.Services.Bybit.Monitors;
using Infrastructure.Services.General;

using MediatR;

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
        
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(IDomainMarker).Assembly, typeof(IApplicationMarker).Assembly, typeof(IInfrastructureMarker).Assembly));
        services.AddValidatorsFromAssemblyContaining<IDomainMarker>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
        
        services.AddDbContext<FuturesTradingDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("TradingHistoryDB")!));
        services.AddRepositories();
        services.AddDataAccessServices();

        services.AddSingleton<IUpdateSubscriptionProxy, UpdateSubscriptionProxy>();
        services.AddSingleton<Func<IUpdateSubscriptionProxy>>(services => () => services.GetRequiredService<IUpdateSubscriptionProxy>());
        
        services.AddBybitServices(configuration);
        services.AddBybitServiceFactories();
    }
    
    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IFuturesOrdersRepository, FuturesOrdersRepository>();
        services.AddScoped<IFuturesPositionsRepository, FuturesPositionsRepository>();
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
        services.AddSingleton<ApiCredentials>(_ => new ApiCredentials(configuration["Bybit:ApiCredentials:Key"]!, configuration["Bybit:ApiCredentials:Secret"]!));
        
        // bybit client
        services.AddSingleton<IBybitClient, BybitClient>(services =>
        {
            var options = new BybitClientOptions
            {
                UsdPerpetualApiOptions = new RestApiClientOptions
                {
                    BaseAddress = configuration["Bybit:UsdPerpetualApiOptions:BaseAddress"]!,
                },
            };
            
            var client = new BybitClient(options);
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
            var options = new BybitSocketClientOptions
            {
                UsdPerpetualStreamsOptions = new BybitSocketApiClientOptions
                {
                    BaseAddress = configuration["Bybit:UsdPerpetualStreamsOptions:BaseAddress"]!,
                },
            };

            var client = new BybitSocketClient(options);
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
