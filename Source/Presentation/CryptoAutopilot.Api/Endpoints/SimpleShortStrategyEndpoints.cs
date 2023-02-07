using Application.Interfaces.Services.Trading;

using Binance.Net.Enums;

using CryptoAutopilot.Api.Endpoints.Internal.Automation.Strategies;
using CryptoAutopilot.Api.Factories;

using Domain.Models;

using Infrastructure.Strategies.SimpleStrategy;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace CryptoAutopilot.Api.Endpoints;

public class SimpleShortStrategyEndpoints : IStrategyEndpoints<SimpleShortStrategyEngine>
{
    public static void AddStrategy(IServiceCollection services, IConfiguration configuration)
    {
        var currencyPair = new CurrencyPair("ETH", "USDT");
        var timeframe = KlineInterval.OneMinute;
        var margin = 20m;
        var stopLossParameter = 1.01m;
        var takeProfitParameter = 0.99m;
        var leverage = 10;

        services.AddSingleton(services =>
            new SimpleShortStrategyEngine(
               currencyPair,
               timeframe,
               margin,
               stopLossParameter,
               takeProfitParameter,
               services.GetRequiredService<ICfdTradingServiceFactory>().Create(currencyPair, leverage, services),
               services.GetRequiredService<ICfdMarketDataProvider>(),
               services.GetRequiredService<IFuturesMarketsCandlestickAwaiterFactory>().Create(currencyPair, timeframe, services),
               services.GetRequiredService<IMediator>()));
    }

    public static void MapStrategySignalsEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("SimpleShortStrategyCfdUp", ([FromServices] SimpleShortStrategyEngine engine) =>
        {
            engine.CFDMovingUp();
            return Results.Ok();
        }).WithTags(nameof(SimpleShortStrategyEngine));

        app.MapPost("SimpleShortStrategyCfdDown", ([FromServices] SimpleShortStrategyEngine engine) =>
        {
            engine.CFDMovingDown();
            return Results.Ok();
        }).WithTags(nameof(SimpleShortStrategyEngine));
    }

}
