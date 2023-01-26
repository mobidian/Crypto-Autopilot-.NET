using Application.Interfaces.Services.Trading;

using Binance.Net.Interfaces.Clients;
using Binance.Net.Interfaces.Clients.UsdFuturesApi;

using Domain.Models;

using Infrastructure.Services.Trading;

namespace Presentation.Api.Factories;

public class ICfdTradingServiceFactory
{
    public ICfdTradingService Create(CurrencyPair currencyPair, decimal leverage, IServiceProvider services)
        => new BinanceCfdTradingService(
            currencyPair,
            leverage,
            services.GetRequiredService<IBinanceClient>(),
            services.GetRequiredService<IBinanceClientUsdFuturesApi>(),
            services.GetRequiredService<IBinanceClientUsdFuturesApiTrading>(),
            services.GetRequiredService<IBinanceClientUsdFuturesApiExchangeData>());
}
