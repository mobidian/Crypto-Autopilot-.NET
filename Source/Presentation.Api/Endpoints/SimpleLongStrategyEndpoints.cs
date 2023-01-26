using Application.Interfaces.Services.Trading;

using Binance.Net.Enums;

using Domain.Models;

using Infrastructure.Strategies.SimpleStrategy;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using Presentation.Api.Endpoints.Internal;
using Presentation.Api.Factories;

namespace Presentation.Api.Endpoints;

public class SimpleLongStrategyEndpoints : IEndpoints
{
    public static void AddServices(IServiceCollection services, IConfiguration configuration)
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

    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        MapStartStopEndpoints(app);
        MapStrategySignalsEndpoints(app);
    }
    private static void MapStartStopEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("StartSimpleLongStrategy", async ([FromServices] SimpleLongStrategyEngine engine, IServiceProvider services) => await engine.TryAwaitStartupAsync(services, TimeSpan.FromSeconds(15)));

        app.MapPost("StopSimpleLongStrategy", async ([FromServices] SimpleLongStrategyEngine engine, IServiceProvider services) => await engine.TryAwaitShutdownAsync(services, TimeSpan.FromSeconds(15)));
    }
    private static void MapStrategySignalsEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("SimpleLongStrategyCfdUp", ([FromServices] SimpleLongStrategyEngine engine) =>
        {
            engine.CFDMovingUp();
            return Results.Ok();
        });

        app.MapPost("SimpleLongStrategyCfdDown", ([FromServices] SimpleLongStrategyEngine engine) =>
        {
            engine.CFDMovingDown();
            return Results.Ok();
        });
    }
}
