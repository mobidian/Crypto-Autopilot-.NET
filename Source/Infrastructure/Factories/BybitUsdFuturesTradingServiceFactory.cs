using Application.Interfaces.Services.Bybit;

using Domain.Models;

using Infrastructure.Services.Bybit;

using MediatR;

using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Factories;

public class BybitUsdFuturesTradingServiceFactory
{
    public IBybitUsdFuturesTradingService Create(CurrencyPair CurrencyPair, decimal Leverage, IServiceProvider services)
        => new BybitUsdFuturesTradingService(
            CurrencyPair,
            Leverage,
            services.GetRequiredService<IBybitFuturesAccountDataProvider>(),
            services.GetRequiredService<IBybitUsdFuturesMarketDataProvider>(),
            services.GetRequiredService<IBybitUsdFuturesTradingApiClient>(),
            services.GetRequiredService<IMediator>());
}
