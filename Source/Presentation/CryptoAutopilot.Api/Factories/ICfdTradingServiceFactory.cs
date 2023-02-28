using Application.Interfaces.Services.Trading;
using Application.Interfaces.Services.Trading.Monitors;

using Binance.Net.Interfaces.Clients.UsdFuturesApi;

using Domain.Models;

using Infrastructure.Services.Trading;

namespace CryptoAutopilot.Api.Factories;

public class ICfdTradingServiceFactory
{
    public ICfdTradingService Create(CurrencyPair currencyPair, decimal leverage, IServiceProvider services)
        => new BinanceCfdTradingService(
            currencyPair,
            leverage,
            services.GetRequiredService<IBinanceClientUsdFuturesApi>(),
            services.GetRequiredService<IBinanceClientUsdFuturesApiTrading>(),
            services.GetRequiredService<IBinanceFuturesAccountDataProvider>(),
            services.GetRequiredService<ICfdMarketDataProvider>(),
            services.GetRequiredService<IOrderStatusMonitor>());
}
