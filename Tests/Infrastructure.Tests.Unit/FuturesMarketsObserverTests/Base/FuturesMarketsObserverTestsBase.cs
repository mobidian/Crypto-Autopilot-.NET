using Binance.Net.Enums;
using Binance.Net.Interfaces.Clients.UsdFuturesApi;

using Domain.Models;

using Infrastructure.Services.Trading;

using NSubstitute;

namespace Infrastructure.Tests.Unit.FuturesMarketsObserverTests.Base;

public abstract class FuturesMarketsObserverTestsBase
{
    protected FuturesMarketsCandlestickAwaiter SUT;
    protected readonly CurrencyPair CurrencyPair = new CurrencyPair("ETH", "BUSD");
    protected readonly IBinanceSocketClientUsdFuturesStreams FuturesStreams = Substitute.For<IBinanceSocketClientUsdFuturesStreams>();

    public FuturesMarketsObserverTestsBase()
    {
        this.SUT = new FuturesMarketsCandlestickAwaiter(this.CurrencyPair, KlineInterval.OneHour, this.FuturesStreams);
    }
}
