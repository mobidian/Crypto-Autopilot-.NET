using Binance.Net.Enums;

using Domain.Models;

using Infrastructure.Services.Trading;
using Infrastructure.Tests.Integration.Common;

namespace Infrastructure.Tests.Integration.BinanceCfdMarketDataProviderTests.Base;

public abstract class BinanceCfdMarketDataProviderTestsBase
{
    protected readonly BinanceCfdMarketDataProvider SUT = new(new("BTC", "BUSD"), Credentials.BinanceIntegrationTestingAPICredentials);
    protected readonly Faker Faker = new Faker();


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
