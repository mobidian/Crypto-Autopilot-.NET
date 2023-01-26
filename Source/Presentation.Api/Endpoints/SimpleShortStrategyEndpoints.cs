using Application.Interfaces.Services.Trading;

using Binance.Net.Enums;

using Domain.Models;

using Infrastructure.Strategies.SimpleStrategy;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using Presentation.Api.Endpoints.Internal;
using Presentation.Api.Factories;

namespace Presentation.Api.Endpoints;

public class SimpleShortStrategyEndpoints : IEndpoints
{
    public static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        var currencyPair = new CurrencyPair("ETH", "USDT");
        var timeframe = KlineInterval.OneMinute;
        var leverage = 10;

        services.AddSingleton<SimpleShortStrategyEngine>(services =>
            new SimpleShortStrategyEngine(
               currencyPair,
               timeframe,
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
        app.MapPost("StartSimpleShortStrategy", async ([FromServices] SimpleShortStrategyEngine engine, IServiceProvider services) => await engine.TryAwaitStartupAsync(services, TimeSpan.FromSeconds(15)));

        app.MapPost("StopSimpleShortStrategy", async ([FromServices] SimpleShortStrategyEngine engine, IServiceProvider services) => await engine.TryAwaitShutdownAsync(services, TimeSpan.FromSeconds(15)));
    }
    private static void MapStrategySignalsEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("SimpleShortStrategyCfdUp", ([FromServices] SimpleShortStrategyEngine engine) =>
        {
            engine.CFDMovingUp();
            return Results.Ok();
        });

        app.MapPost("SimpleShortStrategyCfdDown", ([FromServices] SimpleShortStrategyEngine engine) =>
        {
            engine.CFDMovingDown();
            return Results.Ok();
        });
    }
}
