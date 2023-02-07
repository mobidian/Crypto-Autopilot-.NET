using Application.Interfaces.Services.Trading;

using Binance.Net.Enums;

using CryptoAutopilot.Api.Endpoints.Internal.Automation.Strategies;
using CryptoAutopilot.Api.Factories;

using Domain.Models;

using Infrastructure.Strategies.Example;
using Infrastructure.Strategies.Example.Enums;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace CryptoAutopilot.Api.Endpoints;

public class ExampleStrategyEndpoints : IStrategyEndpoints<ExampleStrategyEngine>
{
    public static void AddStrategy(IServiceCollection services, IConfiguration configuration)
    {
        var currencyPair = new CurrencyPair("ETH", "BUSD");
        var timeframe = KlineInterval.OneMinute;
        var emaLength = 50;
        var margin = 20m;
        var riskRewardRatio = 3;
        var leverage = 10;

        services.AddSingleton(services =>
            new ExampleStrategyEngine(
               currencyPair,
               timeframe,
               emaLength,
               margin,
               riskRewardRatio,
               services.GetRequiredService<ICfdTradingServiceFactory>().Create(currencyPair, leverage, services),
               services.GetRequiredService<ICfdMarketDataProvider>(),
               services.GetRequiredService<IFuturesMarketsCandlestickAwaiterFactory>().Create(currencyPair, timeframe, services),
               services.GetRequiredService<IMediator>()));
    }

    public static void MapStrategySignalsEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("ExampleStrategy", ([FromServices] ExampleStrategyEngine engine, [FromQuery] RsiDivergence divergence) => engine.FlagDivergence(divergence)).WithTags(nameof(ExampleStrategyEngine));
    }
}
