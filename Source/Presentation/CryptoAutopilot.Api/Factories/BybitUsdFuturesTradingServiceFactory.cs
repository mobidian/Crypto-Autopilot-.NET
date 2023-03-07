using Application.Interfaces.Services.Trading.Bybit;

using Domain.Models;

using Infrastructure.Services.Trading.Bybit;

namespace CryptoAutopilot.Api.Factories;

public class BybitUsdFuturesTradingServiceFactory
{
    public IBybitUsdFuturesTradingService Create(CurrencyPair CurrencyPair, decimal Leverage, IServiceProvider services)
        => new BybitUsdFuturesTradingService(
            CurrencyPair,
            Leverage,
            services.GetRequiredService<IBybitFuturesAccountDataProvider>(),
            services.GetRequiredService<IBybitUsdFuturesMarketDataProvider>(),
            services.GetRequiredService<IBybitUsdFuturesTradingApiClient>());
}
