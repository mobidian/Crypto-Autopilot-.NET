﻿using System.Text;

using Application.DataAccess.Repositories;
using Application.DataAccess.Services;
using Application.Interfaces.Logging;
using Application.Interfaces.Proxies;
using Application.Interfaces.Services.Bybit;
using Application.Interfaces.Services.Bybit.Monitors;
using Application.Interfaces.Services.General;

using Bybit.Net.Clients;
using Bybit.Net.Clients.UsdPerpetualApi;
using Bybit.Net.Interfaces.Clients;
using Bybit.Net.Interfaces.Clients.UsdPerpetualApi;

using CryptoAutopilot.Api;
using CryptoAutopilot.Api.Services;
using CryptoAutopilot.Api.Services.Interfaces;

using Domain.PipelineBehaviors;

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

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace Presentation.Api.Tests.Integration.DependencyValidationTests;

public class DependencyRegistrationTests
{
    private readonly List<(Type? serviceType, Type? implementationType, ServiceLifetime? lifetime)> RequiredDescriptors = new();

    public DependencyRegistrationTests()
    {
        this.RequiredDescriptors.Add((typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>), ServiceLifetime.Singleton));
        this.RequiredDescriptors.Add((typeof(IDateTimeProvider), typeof(DateTimeProvider), ServiceLifetime.Singleton));

        // AddMediatR
        // AddValidatorsFromAssemblyContaining<IDomainMarker>
        this.RequiredDescriptors.Add((typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>), ServiceLifetime.Transient));

        this.RequiredDescriptors.Add((typeof(FuturesTradingDbContext), typeof(FuturesTradingDbContext), ServiceLifetime.Scoped));
        this.AddRepositories();
        this.AddDataAccessServices();

        this.RequiredDescriptors.Add((typeof(IUpdateSubscriptionProxy), typeof(UpdateSubscriptionProxy), ServiceLifetime.Singleton));
        this.RequiredDescriptors.Add((typeof(Func<IUpdateSubscriptionProxy>), typeof(Func<IUpdateSubscriptionProxy>), ServiceLifetime.Singleton));

        this.AddBybitServices();
        this.AddBybitServiceFactories();

        this.RequiredDescriptors.Add((typeof(IStrategiesTracker), typeof(StrategiesTracker), ServiceLifetime.Singleton));
    }

    private void AddRepositories()
    {
        this.RequiredDescriptors.Add((typeof(IFuturesOrdersRepository), typeof(FuturesOrdersRepository), ServiceLifetime.Scoped));
        this.RequiredDescriptors.Add((typeof(IFuturesPositionsRepository), typeof(FuturesPositionsRepository), ServiceLifetime.Scoped));
    }

    private void AddDataAccessServices()
    {
        this.RequiredDescriptors.Add((typeof(IFuturesOperationsService), typeof(FuturesOperationsService), ServiceLifetime.Scoped));
    }

    private void AddBybitServices()
    {
        this.RequiredDescriptors.Add((typeof(IBybitRestClient), typeof(BybitRestClient), ServiceLifetime.Singleton));
        this.RequiredDescriptors.Add((typeof(IBybitRestClientUsdPerpetualApi), typeof(BybitRestClientUsdPerpetualApi), ServiceLifetime.Singleton));
        this.RequiredDescriptors.Add((typeof(IBybitRestClientUsdPerpetualApiTrading), typeof(BybitRestClientUsdPerpetualApiTrading), ServiceLifetime.Singleton));
        this.RequiredDescriptors.Add((typeof(IBybitRestClientUsdPerpetualApiExchangeData), typeof(BybitRestClientUsdPerpetualApiExchangeData), ServiceLifetime.Singleton));
        this.RequiredDescriptors.Add((typeof(IBybitRestClientUsdPerpetualApiExchangeData), typeof(BybitRestClientUsdPerpetualApiExchangeData), ServiceLifetime.Singleton));

        this.RequiredDescriptors.Add((typeof(IBybitSocketClient), typeof(BybitSocketClient), ServiceLifetime.Singleton));
        this.RequiredDescriptors.Add((typeof(IBybitSocketClientUsdPerpetualApi), typeof(BybitSocketClientUsdPerpetualApi), ServiceLifetime.Singleton));

        this.RequiredDescriptors.Add((typeof(IBybitFuturesAccountDataProvider), typeof(BybitFuturesAccountDataProvider), ServiceLifetime.Singleton));
        this.RequiredDescriptors.Add((typeof(IBybitUsdFuturesMarketDataProvider), typeof(BybitUsdFuturesMarketDataProvider), ServiceLifetime.Singleton));
        this.RequiredDescriptors.Add((typeof(IBybitUsdFuturesTradingApiClient), typeof(BybitUsdFuturesTradingApiClient), ServiceLifetime.Singleton));
        this.RequiredDescriptors.Add((typeof(IBybitUsdPerpetualKlinesMonitor), typeof(BybitUsdPerpetualKlinesMonitor), ServiceLifetime.Singleton));
    }
    private void AddBybitServiceFactories()
    {
        this.RequiredDescriptors.Add((typeof(BybitUsdFuturesTradingServiceFactory), typeof(BybitUsdFuturesTradingServiceFactory), ServiceLifetime.Singleton));
    }


    [Fact]
    public void RegistrationValidation()
    {
        var app = new WebApplicationFactory<IApiMarker>()
            .WithWebHostBuilder(builder =>
            builder.ConfigureServices(serviceCollection =>
            {
                var services = serviceCollection.ToList();
                var res = this.ValidateServices(services);

                if (!res.Success)
                    Assert.Fail(res.Message);

                // Assert.Pass();
            }));

        app.CreateClient(); // triggers the building of the application
    }
    private (bool Success, string Message) ValidateServices(List<ServiceDescriptor> serviceDescriptors)
    {
        var searchFailed = false;
        var failedText = new StringBuilder();

        foreach (var (serviceType, implementationType, lifetime) in this.RequiredDescriptors)
        {
            var match = serviceDescriptors.SingleOrDefault(serviceDescriptor =>
                serviceDescriptor.ServiceType == serviceType &&
                (serviceDescriptor.ImplementationType == implementationType || serviceDescriptor.ImplementationFactory is not null || serviceDescriptor.ImplementationInstance is not null) &&
                serviceDescriptor.Lifetime == lifetime);

            if (match is not null)
                continue;

            if (!searchFailed)
            {
                failedText.AppendLine("Did not find registered service for:");
                searchFailed = true;
            }

            failedText.AppendLine($"{serviceType} | {implementationType} | {lifetime}");
        }

        return (!searchFailed, failedText.ToString());
    }
}
