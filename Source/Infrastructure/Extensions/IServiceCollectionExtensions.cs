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

using Domain;
using Domain.PipelineBehaviors;

using FluentValidation;

using Infrastructure.DataAccess.Database;
using Infrastructure.DataAccess.Repositories;
using Infrastructure.DataAccess.Services;
using Infrastructure.Factories;
using Infrastructure.Logging;
using Infrastructure.Options;
using Infrastructure.Proxies;
using Infrastructure.Services.Bybit;
using Infrastructure.Services.Bybit.Monitors;
using Infrastructure.Services.General;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
        services.AddOptions<BybitEnvironmentOptions>().Bind(configuration.GetRequiredSection(BybitEnvironmentOptions.SectionName));
        services.AddOptions<HmacApiCredentialsOptions>().Bind(configuration.GetRequiredSection(HmacApiCredentialsOptions.SectionName));
        services.AddOptions<ApiKeyPermissionsOptions>().Bind(configuration.GetRequiredSection(ApiKeyPermissionsOptions.SectionName));

        services.AddSingleton<IBybitRestClient, BybitRestClient>(services => new BybitRestClient(options =>
        {
            options.Environment = services.GetRequiredService<IOptions<BybitEnvironmentOptions>>().Value.GetEnvironment();
            options.ApiCredentials = services.GetRequiredService<IOptions<HmacApiCredentialsOptions>>().Value.GetApiCredentials();
        }));
        services.AddSingleton<IBybitRestClientUsdPerpetualApi>(services => services.GetRequiredService<IBybitRestClient>().UsdPerpetualApi);
        services.AddSingleton<IBybitRestClientUsdPerpetualApiTrading>(services => services.GetRequiredService<IBybitRestClientUsdPerpetualApi>().Trading);
        services.AddSingleton<IBybitRestClientUsdPerpetualApiExchangeData>(services => services.GetRequiredService<IBybitRestClientUsdPerpetualApi>().ExchangeData);
        services.AddSingleton<IBybitRestClientUsdPerpetualApiAccount>(services => services.GetRequiredService<IBybitRestClientUsdPerpetualApi>().Account);

        services.AddSingleton<IBybitSocketClient, BybitSocketClient>(services => new BybitSocketClient(options =>
        {
            options.Environment = services.GetRequiredService<IOptions<BybitEnvironmentOptions>>().Value.GetEnvironment();
            options.ApiCredentials = services.GetRequiredService<IOptions<HmacApiCredentialsOptions>>().Value.GetApiCredentials();
        }));
        services.AddSingleton<IBybitSocketClientUsdPerpetualApi>(services => services.GetRequiredService<IBybitSocketClient>().UsdPerpetualApi);

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
