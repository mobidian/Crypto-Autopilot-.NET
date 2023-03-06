using Application.Interfaces.Services.Trading.BybitExchange;

using Domain.Models;

using Infrastructure.Services.Trading.BybitExchange;

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
