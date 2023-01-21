﻿using System.Text;

using Application.Interfaces.Logging;
using Application.Interfaces.Proxies;
using Application.Interfaces.Services.General;
using Application.Interfaces.Services.Trading;

using Binance.Net.Clients;
using Binance.Net.Clients.UsdFuturesApi;
using Binance.Net.Enums;
using Binance.Net.Interfaces.Clients;
using Binance.Net.Interfaces.Clients.UsdFuturesApi;

using CryptoExchange.Net.Authentication;

using Domain.Models;

using Infrastructure.Database.Contexts;
using Infrastructure.Logging;
using Infrastructure.Services.General;
using Infrastructure.Services.Proxies;
using Infrastructure.Services.Trading;
using Infrastructure.Strategies.SimpleStrategy;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Presentation.Api.Tests.Integration.DependencyValidationTests;

public class DependencyRegistrationTests
{
    private readonly List<(Type? serviceType, Type? implementationType, ServiceLifetime? lifetime)> RequiredDescriptors = new()
    {
        (typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>), ServiceLifetime.Singleton),
        (typeof(IDateTimeProvider), typeof(DateTimeProvider), ServiceLifetime.Singleton),

        (typeof(FuturesTradingDbContext), typeof(FuturesTradingDbContext), ServiceLifetime.Singleton),
        (typeof(IFuturesTradesDBService), typeof(FuturesTradesDBService), ServiceLifetime.Singleton),

        #region Binance
		(typeof(CurrencyPair), typeof(CurrencyPair), ServiceLifetime.Transient),
        (typeof(ApiCredentials), typeof(ApiCredentials), ServiceLifetime.Transient),
        (typeof(KlineInterval), typeof(KlineInterval), ServiceLifetime.Singleton),
        (typeof(decimal), typeof(decimal), ServiceLifetime.Singleton),


        #region From binance clients
		(typeof(IBinanceClient), typeof(BinanceClient), ServiceLifetime.Transient),
        (typeof(IBinanceClientUsdFuturesApi), typeof(BinanceClientUsdFuturesApi), ServiceLifetime.Singleton),
        (typeof(IBinanceClientUsdFuturesApiTrading), typeof(BinanceClientUsdFuturesApiTrading), ServiceLifetime.Singleton),
        (typeof(IBinanceClientUsdFuturesApiExchangeData), typeof(BinanceClientUsdFuturesApiExchangeData), ServiceLifetime.Singleton),

        (typeof(IBinanceSocketClient), typeof(BinanceSocketClient), ServiceLifetime.Transient),
        (typeof(IBinanceSocketClientUsdFuturesStreams), typeof(BinanceSocketClientUsdFuturesStreams), ServiceLifetime.Singleton), 
	    #endregion


        (typeof(ICfdMarketDataProvider), typeof(BinanceCfdMarketDataProvider), ServiceLifetime.Singleton),
        (typeof(ICfdTradingService), typeof(BinanceCfdTradingService), ServiceLifetime.Singleton),
        
        (typeof(IUpdateSubscriptionProxy), typeof(UpdateSubscriptionProxy), ServiceLifetime.Singleton),
        (typeof(IFuturesMarketsCandlestickAwaiter), typeof(FuturesMarketsCandlestickAwaiter), ServiceLifetime.Singleton),

	    #endregion

        (typeof(SimpleStrategyEngine), typeof(SimpleStrategyEngine), ServiceLifetime.Singleton),
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