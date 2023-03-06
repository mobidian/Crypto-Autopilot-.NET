using System.Text;

using Application.Interfaces.Logging;
using Application.Interfaces.Proxies;
using Application.Interfaces.Services.General;
using Application.Interfaces.Services.Trading.Binance;
using Application.Interfaces.Services.Trading.BybitExchange;
using Application.Interfaces.Services.Trading.BybitExchange.Monitors;

using Bybit.Net.Clients;
using Bybit.Net.Clients.UsdPerpetualApi;
using Bybit.Net.Interfaces.Clients;
using Bybit.Net.Interfaces.Clients.UsdPerpetualApi;

using CryptoAutopilot.Api;
using CryptoAutopilot.Api.Factories;
using CryptoAutopilot.Api.Services;
using CryptoAutopilot.Api.Services.Interfaces;

using CryptoExchange.Net.Authentication;

using Infrastructure.Database.Contexts;
using Infrastructure.Logging;
using Infrastructure.Services.General;
using Infrastructure.Services.Proxies;
using Infrastructure.Services.Trading;
using Infrastructure.Services.Trading.BybitExchange;
using Infrastructure.Services.Trading.BybitExchange.Monitors;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Presentation.Api.Tests.Integration.DependencyValidationTests;

public class DependencyRegistrationTests
{
    private readonly List<(Type? serviceType, Type? implementationType, ServiceLifetime? lifetime)> RequiredDescriptors = new()
    {
        #region AddServices
        (typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>), ServiceLifetime.Singleton),
        (typeof(IDateTimeProvider), typeof(DateTimeProvider), ServiceLifetime.Singleton),

        (typeof(FuturesTradingDbContext), typeof(FuturesTradingDbContext), ServiceLifetime.Transient),
        (typeof(IFuturesTradesDBService), typeof(FuturesTradesDBService), ServiceLifetime.Transient),
        
        (typeof(IUpdateSubscriptionProxy), typeof(UpdateSubscriptionProxy), ServiceLifetime.Singleton),
        (typeof(Func<IUpdateSubscriptionProxy>), typeof(Func<IUpdateSubscriptionProxy>), ServiceLifetime.Singleton),

        
        #region AddBybitServices
        (typeof(ApiCredentials), typeof(ApiCredentials), ServiceLifetime.Singleton),

        (typeof(IBybitClient), typeof(BybitClient), ServiceLifetime.Singleton),
        (typeof(IBybitClientUsdPerpetualApi), typeof(BybitClientUsdPerpetualApi), ServiceLifetime.Singleton),
        (typeof(IBybitClientUsdPerpetualApiTrading), typeof(BybitClientUsdPerpetualApiTrading), ServiceLifetime.Singleton),
        (typeof(IBybitClientUsdPerpetualApiExchangeData), typeof(BybitClientUsdPerpetualApiExchangeData), ServiceLifetime.Singleton),
        (typeof(IBybitClientUsdPerpetualApiExchangeData), typeof(BybitClientUsdPerpetualApiExchangeData), ServiceLifetime.Singleton),
        
        (typeof(IBybitSocketClient), typeof(BybitSocketClient), ServiceLifetime.Singleton),
        (typeof(IBybitSocketClientUsdPerpetualStreams), typeof(BybitSocketClientUsdPerpetualStreams), ServiceLifetime.Singleton),
        
        (typeof(IBybitFuturesAccountDataProvider), typeof(BybitFuturesAccountDataProvider), ServiceLifetime.Singleton),
        (typeof(IBybitUsdFuturesMarketDataProvider), typeof(BybitUsdFuturesMarketDataProvider), ServiceLifetime.Singleton),
        (typeof(IBybitUsdFuturesTradingApiClient), typeof(BybitUsdFuturesTradingApiClient), ServiceLifetime.Singleton),
        (typeof(IBybitUsdPerpetualKlinesMonitor), typeof(BybitUsdPerpetualKlinesMonitor), ServiceLifetime.Singleton),
        #endregion
        
        #region AddBybitServiceFactories
        (typeof(BybitUsdFuturesTradingServiceFactory), typeof(BybitUsdFuturesTradingServiceFactory), ServiceLifetime.Singleton),
        #endregion


        (typeof(IStrategiesTracker), typeof(StrategiesTracker), ServiceLifetime.Singleton),
        #endregion
    };


    [Test]
    public void RegistrationValidation()
    {
        var app = new WebApplicationFactory<IApiMarker>()
            .WithWebHostBuilder(builder =>
            builder.ConfigureServices(serviceCollection =>
            {
                var services = serviceCollection.ToList();
                var res = ValidateServices(services);

                if (!res.Success)
                    Assert.Fail(res.Message);

                Assert.Pass();
            }));
        
        app.CreateClient(); // triggers the building of the application
    }
    private (bool Success, string Message) ValidateServices(List<ServiceDescriptor> serviceDescriptors)
    {
        var searchFailed = false;
        var failedText = new StringBuilder();
        var failedText2 = new StringBuilder();
        
        foreach (var requiredDescriptor in this.RequiredDescriptors)
        {
            var match = serviceDescriptors.SingleOrDefault(serviceDescriptor => 
                serviceDescriptor.ServiceType == requiredDescriptor.serviceType &&
                (serviceDescriptor.ImplementationType == requiredDescriptor.implementationType || serviceDescriptor.ImplementationFactory is not null || serviceDescriptor.ImplementationInstance is not null) &&
                serviceDescriptor.Lifetime == requiredDescriptor.lifetime);
            
            if (match is not null)
                continue;
            
            if (!searchFailed)
            {
                failedText.AppendLine("Did not find registered service for:");
                searchFailed = true;
            }
            
            failedText.AppendLine($"{requiredDescriptor.serviceType} | {requiredDescriptor.implementationType} | {requiredDescriptor.lifetime}");
        }
        
        return (!searchFailed, failedText.ToString());
    }
}
