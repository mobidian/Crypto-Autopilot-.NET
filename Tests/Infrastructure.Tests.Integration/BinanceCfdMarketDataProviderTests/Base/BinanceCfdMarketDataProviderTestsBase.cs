using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Interfaces.Clients;
using Binance.Net.Interfaces.Clients.UsdFuturesApi;
using Binance.Net.Objects;

using Domain.Models;

using Infrastructure.Services.Trading;
using Infrastructure.Tests.Integration.Common;

namespace Infrastructure.Tests.Integration.BinanceCfdMarketDataProviderTests.Base;

public abstract class BinanceCfdMarketDataProviderTestsBase
{
    private readonly SecretsManager SecretsManager = new SecretsManager();

    protected readonly CurrencyPair CurrencyPair = new CurrencyPair("ETH", "BUSD");

    protected readonly BinanceCfdMarketDataProvider SUT;
    private readonly IBinanceClient BinanceClient;
    private readonly IBinanceClientUsdFuturesApi FuturesClient;
    private readonly IBinanceClientUsdFuturesApiExchangeData FuturesExchangeData;

    public BinanceCfdMarketDataProviderTestsBase()
    {
        this.BinanceClient = new BinanceClient();
        this.BinanceClient.SetApiCredentials(new BinanceApiCredentials(this.SecretsManager.GetSecret("BinanceApiCredentials:key"), this.SecretsManager.GetSecret("BinanceApiCredentials:secret")));

        this.FuturesClient = this.BinanceClient.UsdFuturesApi;
        this.FuturesExchangeData = this.FuturesClient.ExchangeData;
        
        this.SUT = new BinanceCfdMarketDataProvider(this.BinanceClient, this.FuturesClient, this.FuturesExchangeData);
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

    
    protected bool CandlesticksAreTimelyConsistent(IEnumerable<Candlestick> candlesticks, KlineInterval timeframe)
    {
        var timeInterval = TimeSpan.FromSeconds((int)timeframe);
        
        return candlesticks
        .Zip(candlesticks.Skip(1), (current, next) => next.Date - current.Date)
        .All(delta => delta == timeInterval);
    }
}
