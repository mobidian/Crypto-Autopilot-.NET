using Application.Interfaces.Logging;
using Application.Interfaces.Proxies;
using Application.Interfaces.Services.Trading;

using Binance.Net.Enums;
using Binance.Net.Interfaces.Clients.UsdFuturesApi;

using Domain.Models;

using Infrastructure.Services.Trading;

namespace CryptoAutopilot.Api.Factories;

public class IFuturesMarketsCandlestickAwaiterFactory
{
    public IFuturesMarketsCandlestickAwaiter Create(CurrencyPair currencyPair, KlineInterval timeframe, IServiceProvider services)
        => new FuturesMarketsCandlestickAwaiter(
            currencyPair,
            timeframe,
            services.GetRequiredService<IBinanceSocketClientUsdFuturesStreams>(),
            services.GetRequiredService<IUpdateSubscriptionProxy>(),
            services.GetRequiredService<ILoggerAdapter<FuturesMarketsCandlestickAwaiter>>());
}
