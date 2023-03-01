using Application.Interfaces.Services.Trading;
using Application.Interfaces.Services.Trading.Monitors;

using Binance.Net.Interfaces.Clients.UsdFuturesApi;

using Domain.Models;

using Infrastructure.Services.Trading;

namespace CryptoAutopilot.Api.Factories;

public class FuturesTradingServiceFactory
{
    public IFuturesTradingService Create(CurrencyPair currencyPair, decimal leverage, IServiceProvider services)
        => new BinanceFuturesTradingService(
            currencyPair,
            leverage,
            services.GetRequiredService<IBinanceClientUsdFuturesApiTrading>(),
            services.GetRequiredService<IBinanceFuturesAccountDataProvider>(),
            services.GetRequiredService<IFuturesMarketDataProvider>(),
            services.GetRequiredService<IOrderStatusMonitor>());
}
