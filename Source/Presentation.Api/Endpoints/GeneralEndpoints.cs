using Application.Interfaces.Logging;
using Application.Interfaces.Proxies;
using Application.Interfaces.Services.General;
using Application.Interfaces.Services.Trading;

using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Interfaces.Clients;
using Binance.Net.Interfaces.Clients.UsdFuturesApi;

using CryptoExchange.Net.Authentication;

using Domain.Models;

using Infrastructure;
using Infrastructure.Database.Contexts;
using Infrastructure.Logging;
using Infrastructure.Services.General;
using Infrastructure.Services.Proxies;
using Infrastructure.Services.Trading;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using Presentation.Api.Contracts.Responses;

namespace Presentation.Api.Endpoints;

public static class GeneralEndpoints
{
    public static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>));
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        
        services.AddMediatR(typeof(IInfrastructureMarker).Assembly);

        services.AddTransient<FuturesTradingDbContext>(services => new FuturesTradingDbContext(configuration.GetConnectionString("CryptoPilotTrades")!, services.GetRequiredService<IDateTimeProvider>()));
        services.AddTransient<IFuturesTradesDBService, FuturesTradesDBService>();

        AddBinanceRelatedServices(services, configuration);
    }
    private static void AddBinanceRelatedServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<CurrencyPair>(_ => new CurrencyPair("ETH", "BUSD"));
        services.AddTransient<ApiCredentials>(_ => new ApiCredentials(configuration.GetValue<string>("BinanceApiCredentials:key")!, configuration.GetValue<string>("BinanceApiCredentials:secret")!));
        services.AddSingleton(typeof(KlineInterval), KlineInterval.OneMinute);
        services.AddSingleton(typeof(decimal), 10m);
        
        AddServicesDerivedFromBinanceClients(services);

        services.AddSingleton<ICfdMarketDataProvider, BinanceCfdMarketDataProvider>();
        services.AddSingleton<ICfdTradingService, BinanceCfdTradingService>();
        
        services.AddSingleton<IUpdateSubscriptionProxy, UpdateSubscriptionProxy>();
        services.AddSingleton<IFuturesMarketsCandlestickAwaiter, FuturesMarketsCandlestickAwaiter>();
    }
    private static void AddServicesDerivedFromBinanceClients(IServiceCollection services)
    {
        // binance client
        services.AddTransient<IBinanceClient, BinanceClient>(services =>
        {
            var client = new BinanceClient();
            client.SetApiCredentials(services.GetRequiredService<ApiCredentials>());
            return client;
        });
        services.AddSingleton<IBinanceClientUsdFuturesApi>(services => services.GetRequiredService<IBinanceClient>().UsdFuturesApi);
        services.AddSingleton<IBinanceClientUsdFuturesApiTrading>(services => services.GetRequiredService<IBinanceClientUsdFuturesApi>().Trading);
        services.AddSingleton<IBinanceClientUsdFuturesApiExchangeData>(services => services.GetRequiredService<IBinanceClientUsdFuturesApi>().ExchangeData);
        
        // binance socket client
        services.AddTransient<IBinanceSocketClient, BinanceSocketClient>(services =>
        {
            var socketClient = new BinanceSocketClient();
            socketClient.SetApiCredentials(services.GetRequiredService<ApiCredentials>());
            return socketClient;
        });
        services.AddSingleton<IBinanceSocketClientUsdFuturesStreams>(services => services.GetRequiredService<IBinanceSocketClient>().UsdFuturesStreams);
    }

    public static void MapEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("candlesticks", async ([FromServices] IFuturesTradesDBService DBService) => Results.Ok(new GetAllCandlesticksResponse { Candlesticks = await DBService.GetAllCandlesticksAsync() }));
        
        app.MapGet("futuresorders", async ([FromServices] IFuturesTradesDBService DBService) => Results.Ok(new GetAllFuturesOrdersResponse { FuturesOrders = await DBService.GetAllFuturesOrdersAsync() }));
    }
}
