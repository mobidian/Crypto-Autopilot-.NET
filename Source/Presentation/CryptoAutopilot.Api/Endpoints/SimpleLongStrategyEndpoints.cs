using Application.Interfaces.Services.Trading;

using Binance.Net.Enums;

using Domain.Models;

using Infrastructure.Strategies.SimpleStrategy;

using MediatR;

using Microsoft.AspNetCore.Mvc;
using Presentation.Api.Endpoints.Internal.Automation.Strategies;
using Presentation.Api.Factories;

namespace Presentation.Api.Endpoints;

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

        services.AddSingleton<SimpleLongStrategyEngine>(services =>
            new SimpleLongStrategyEngine(
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
