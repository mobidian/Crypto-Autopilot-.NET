using System.Text;

using Application.Interfaces.Logging;
using Application.Interfaces.Proxies;
using Application.Interfaces.Services.General;
using Application.Interfaces.Services.Trading.Binance;
using Application.Interfaces.Services.Trading.Binance.Monitors;

using Binance.Net.Clients;
using Binance.Net.Clients.UsdFuturesApi;
using Binance.Net.Interfaces.Clients;
using Binance.Net.Interfaces.Clients.UsdFuturesApi;
using Binance.Net.Objects;

using CryptoAutopilot.Api;
using CryptoAutopilot.Api.Factories;
using CryptoAutopilot.Api.Services;
using CryptoAutopilot.Api.Services.Interfaces;

using Infrastructure.Database.Contexts;
using Infrastructure.Logging;
using Infrastructure.Services.General;
using Infrastructure.Services.Proxies;
using Infrastructure.Services.Trading.Binance;
using Infrastructure.Services.Trading.Binance.Monitors;
using Infrastructure.Strategies.SimpleStrategy;

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



        (typeof(IFuturesMarketDataProvider), typeof(BinanceFuturesMarketDataProvider), ServiceLifetime.Singleton),
        (typeof(IUpdateSubscriptionProxy), typeof(UpdateSubscriptionProxy), ServiceLifetime.Singleton),


        #region AddBinanceClientsAndServicesDerivedFromThem
        (typeof(BinanceApiCredentials), typeof(BinanceApiCredentials), ServiceLifetime.Transient),

        (typeof(IBinanceClient), typeof(BinanceClient), ServiceLifetime.Transient),
        (typeof(IBinanceClientUsdFuturesApi), typeof(BinanceClientUsdFuturesApi), ServiceLifetime.Transient),
        (typeof(IBinanceClientUsdFuturesApiTrading), typeof(BinanceClientUsdFuturesApiTrading), ServiceLifetime.Transient),
        (typeof(IBinanceClientUsdFuturesApiExchangeData), typeof(BinanceClientUsdFuturesApiExchangeData), ServiceLifetime.Transient),

        (typeof(IBinanceSocketClient), typeof(BinanceSocketClient), ServiceLifetime.Transient),
        (typeof(IBinanceSocketClientUsdFuturesStreams), typeof(BinanceSocketClientUsdFuturesStreams), ServiceLifetime.Transient),
        #endregion

        (typeof(IOrderStatusMonitor), typeof(OrderStatusMonitor), ServiceLifetime.Singleton),
        (typeof(IFuturesCandlesticksMonitor), typeof(FuturesCandlesticksMonitor), ServiceLifetime.Singleton),
        
        (typeof(IBinanceFuturesAccountDataProvider), typeof(BinanceFuturesAccountDataProvider), ServiceLifetime.Singleton),

        #region AddServiceFactories
        (typeof(FuturesTradingServiceFactory), typeof(FuturesTradingServiceFactory), ServiceLifetime.Singleton),
        (typeof(Func<IUpdateSubscriptionProxy>), typeof(Func<IUpdateSubscriptionProxy>), ServiceLifetime.Singleton),
	    #endregion
        
        (typeof(IStrategiesTracker), typeof(StrategiesTracker), ServiceLifetime.Singleton),
        #endregion


        (typeof(SimpleLongStrategyEngine), typeof(SimpleLongStrategyEngine), ServiceLifetime.Singleton),
        (typeof(SimpleShortStrategyEngine), typeof(SimpleShortStrategyEngine), ServiceLifetime.Singleton),
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
