using Application.Interfaces.Services.Trading.Binance;
using Application.Interfaces.Services.Trading.Binance.Monitors;

using Domain.Models;

using Infrastructure.Services.Trading.Binance;

namespace CryptoAutopilot.Api.Factories;

public class FuturesTradingServiceFactory
{
    public IFuturesTradingService Create(CurrencyPair currencyPair, decimal leverage, IServiceProvider services)
        => new BinanceFuturesTradingService(
            currencyPair,
            leverage,
            services.GetRequiredService<IBinanceFuturesApiService>(),
            services.GetRequiredService<IBinanceFuturesAccountDataProvider>(),
            services.GetRequiredService<IFuturesMarketDataProvider>(),
            services.GetRequiredService<IOrderStatusMonitor>());
}
