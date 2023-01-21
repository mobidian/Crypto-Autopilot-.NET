using Binance.Net.Clients;
using Binance.Net.Enums;

using CryptoExchange.Net.Authentication;

using Domain.Models;

using Infrastructure.Services.Trading;
using Infrastructure.Tests.Integration.Common;

namespace Infrastructure.Tests.Integration.BinanceCfdMarketDataProviderTests.Base;

public abstract class BinanceCfdMarketDataProviderTestsBase
{
    private readonly SecretsManager SecretsManager = new SecretsManager();

    protected readonly BinanceCfdMarketDataProvider SUT;
    protected readonly Faker Faker = new Faker();

    public BinanceCfdMarketDataProviderTestsBase()
    {
        var binanceClient = new BinanceClient();
        binanceClient.SetApiCredentials(new ApiCredentials(this.SecretsManager.GetSecret("BinanceApiCredentials:key"), this.SecretsManager.GetSecret("BinanceApiCredentials:secret")));

        this.SUT = new BinanceCfdMarketDataProvider(new CurrencyPair("BTC", "BUSD"), binanceClient, binanceClient.UsdFuturesApi);
    }


    protected static IEnumerable<KlineInterval> GetValidKlineIntervals()
    {
        return Enum.GetValues<KlineInterval>().Except(new[] {
            KlineInterval.OneSecond,
            KlineInterval.ThreeDay,
            KlineInterval.OneMonth });
    }
    protected static IEnumerable<KlineInterval> GetInvalidKlineIntervals()
    {
        yield return KlineInterval.OneSecond;
        yield return KlineInterval.ThreeDay;
        yield return KlineInterval.OneMonth;
    }


    protected bool AreCandlesticksTimelyConsistent(IEnumerable<Candlestick> candlesticks, KlineInterval timeframe)
    {
        TimeSpan timeInterval = TimeSpan.FromSeconds((int)timeframe);

        return candlesticks
        .Zip(candlesticks.Skip(1), (current, next) => next.Date - current.Date)
        .All(delta => delta == timeInterval);
    }
}
