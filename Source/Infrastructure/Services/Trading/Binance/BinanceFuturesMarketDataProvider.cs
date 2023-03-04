using Application.Interfaces.Services.Trading.Binance;

using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Interfaces.Clients.UsdFuturesApi;
using Binance.Net.Objects.Models.Futures;
using Binance.Net.Objects.Models.Spot;

using CryptoExchange.Net.Objects;

using Domain.Models;

using Infrastructure.Extensions;

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Infrastructure.Services.Trading.Binance;

public class BinanceFuturesMarketDataProvider : IFuturesMarketDataProvider
{
    private readonly IBinanceClientUsdFuturesApiTrading TradingClient;
    private readonly IBinanceClientUsdFuturesApiExchangeData FuturesExchangeData;

    public BinanceFuturesMarketDataProvider(IBinanceClientUsdFuturesApiTrading tradingClient, IBinanceClientUsdFuturesApiExchangeData futuresExchangeData)
    {
        this.TradingClient = tradingClient ?? throw new ArgumentNullException(nameof(tradingClient));
        this.FuturesExchangeData = futuresExchangeData ?? throw new ArgumentNullException(nameof(futuresExchangeData));
    }


    public async Task<decimal> GetCurrentPriceAsync(string currencyPair)
    {
        var callResult = await this.FuturesExchangeData.GetPriceAsync(currencyPair);
        callResult.ThrowIfHasError();

        return callResult.Data.Price;
    }

    public async Task<BinanceFuturesOrder> GetOrderAsync(string currencyPair, long orderID)
    {
        var callResult = await this.TradingClient.GetOrderAsync(currencyPair, orderID);
        callResult.ThrowIfHasError();

        return callResult.Data;
    }

    public async Task<IEnumerable<Candlestick>> GetAllCandlesticksAsync(string currencyPair, KlineInterval timeframe)
    {
        ThrowIfUnsupported(timeframe);

        List<BinanceMarkIndexKline> klines;
        List<decimal> volumes;

        DateTime opentime1, opentime2;
        int count1, count2;
        do
        {
            var Task1 = this.FuturesExchangeData.GetMarkPriceKlinesAsync(currencyPair, timeframe);
            var Task2 = this.FuturesExchangeData.GetKlinesAsync(currencyPair, timeframe);

            var marketPriceKlinesCallResult = await Task1;
            var volumesCallResult = await Task2;

            AnalyzeCallResult1(marketPriceKlinesCallResult, out klines, out opentime1, out count1);
            AnalyzeCallResult2(volumesCallResult, out volumes, out opentime2, out count2);
        }
        while (opentime1 != opentime2 || count1 != count2);

        var candlesticks = new List<Candlestick>();
        for (var i = 0; i < klines.Count; i++)
        {
            candlesticks.Add(new Candlestick
            {
                CurrencyPair = currencyPair,

                Date = klines[i].OpenTime,
                Open = klines[i].OpenPrice,
                High = klines[i].HighPrice,
                Low = klines[i].LowPrice,
                Close = klines[i].ClosePrice,
                Volume = volumes[i],
            });
        }

        return candlesticks;
    }
    private static void ThrowIfUnsupported(KlineInterval timeframe)
    {
        // Information from integration tests
        var unsupportedIntervals = new[] { KlineInterval.OneSecond, KlineInterval.ThreeDay, KlineInterval.OneMonth };

        if (unsupportedIntervals.Contains(timeframe))
            throw new NotSupportedException($"The {timeframe} timeframe is not supported");
    }
    private static void AnalyzeCallResult1(WebCallResult<IEnumerable<BinanceMarkIndexKline>> marketPriceKlinesCallResult, out List<BinanceMarkIndexKline> klines, out DateTime opentime1, out int count1)
    {
        marketPriceKlinesCallResult.ThrowIfHasError();
        klines = marketPriceKlinesCallResult.Data.ToList();
        opentime1 = klines.First().OpenTime;
        count1 = klines.Count;
    }
    private static void AnalyzeCallResult2(WebCallResult<IEnumerable<IBinanceKline>> volumesCallResult, out List<decimal> volumes, out DateTime opentime2, out int count2)
    {
        volumesCallResult.ThrowIfHasError();
        volumes = volumesCallResult.Data.Select(x => x.Volume).ToList();
        opentime2 = volumesCallResult.Data.First().OpenTime;
        count2 = volumes.Count;
    }

    public async Task<IEnumerable<Candlestick>> GetCompletedCandlesticksAsync(string currencyPair, KlineInterval timeframe)
    {
        var allCandlesticks = await this.GetAllCandlesticksAsync(currencyPair, timeframe);
        return allCandlesticks.SkipLast(1);
    }

    //// ////

    private bool Disposed = false;

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (this.Disposed)
            return;

        // // nothing to dispose // //

        this.Disposed = true;
    }
}
