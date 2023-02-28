using Application.Interfaces.Services.Trading;
using Application.Interfaces.Services.Trading.Monitors;

using Binance.Net.Enums;

using CryptoAutopilot.Api.Endpoints.Internal.Automation.Strategies;
using CryptoAutopilot.Api.Factories;

using Domain.Models;

using Infrastructure.Services.Trading.Strategies.SimpleStrategy;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace CryptoAutopilot.Api.Endpoints;

public class SimpleLongStrategyEndpoints : IStrategyEndpoints<SimpleLongStrategyEngine>
{
    public static void AddStrategy(IServiceCollection services, IConfiguration configuration)
    {
        var currencyPair = new CurrencyPair("ETH", "BUSD");
        var timeframe = KlineInterval.OneMinute;
        var margin = 20m;
        var stopLossParameter = 0.99m;
        var takeProfitParameter = 1.01m;
        var leverage = 10;

        services.AddSingleton(services =>
            new SimpleLongStrategyEngine(
               currencyPair,
               timeframe,
               margin,
               stopLossParameter,
               takeProfitParameter,
               services.GetRequiredService<ICfdTradingServiceFactory>().Create(currencyPair, leverage, services),
               services.GetRequiredService<ICfdMarketDataProvider>(),
               services.GetRequiredService<IFuturesCandlesticksMonitor>(),
               services.GetRequiredService<IMediator>()));
    }

    public static void MapStrategySignalsEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("SimpleLongStrategyCfdUp", ([FromServices] SimpleLongStrategyEngine engine) =>
        {
            engine.CFDMovingUp();
            return Results.Ok();
        }).WithTags(nameof(SimpleLongStrategyEngine));

        app.MapPost("SimpleLongStrategyCfdDown", ([FromServices] SimpleLongStrategyEngine engine) =>
        {
            engine.CFDMovingDown();
            return Results.Ok();
        }).WithTags(nameof(SimpleLongStrategyEngine));
    }
}
