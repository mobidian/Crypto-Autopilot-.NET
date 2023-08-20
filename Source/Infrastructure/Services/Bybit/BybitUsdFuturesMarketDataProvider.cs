using Application.Extensions;
using Application.Interfaces.Services.Bybit;
using Application.Interfaces.Services.General;

using Bybit.Net.Enums;
using Bybit.Net.Interfaces.Clients.UsdPerpetualApi;
using Bybit.Net.Objects.Models;

namespace Infrastructure.Services.Bybit;

public class BybitUsdFuturesMarketDataProvider : IBybitUsdFuturesMarketDataProvider
{
    private readonly IDateTimeProvider DateTime;
    private readonly IBybitRestClientUsdPerpetualApiExchangeData FuturesExchangeData;

    public BybitUsdFuturesMarketDataProvider(IDateTimeProvider dateTime, IBybitRestClientUsdPerpetualApiExchangeData futuresExchangeData)
    {
        this.DateTime = dateTime ?? throw new ArgumentNullException(nameof(dateTime));
        this.FuturesExchangeData = futuresExchangeData ?? throw new ArgumentNullException(nameof(futuresExchangeData));
    }


    public async Task<decimal> GetLastPriceAsync(string symbol)
    {
        var ticker = await this.GetTickerAsync(symbol);
        return ticker.LastPrice;
    }
    public async Task<decimal?> GetMarketPriceAsync(string symbol)
    {
        var ticker = await this.GetTickerAsync(symbol);
        return ticker.MarkPrice;
    }

    public async Task<BybitTicker> GetTickerAsync(string symbol)
    {
        var callResult = await this.FuturesExchangeData.GetTickerAsync(symbol);
        callResult.ThrowIfHasError();
        return callResult.Data.Single();
    }

    public async Task<IEnumerable<BybitKline>> GetAllCandlesticksAsync(string symbol, KlineInterval timeframe)
    {
        var startTime = this.DateTime.UtcNow.Subtract(TimeSpan.FromSeconds((int)timeframe * 100));
        var callResult = await this.FuturesExchangeData.GetKlinesAsync(symbol, timeframe, startTime, 100);
        callResult.ThrowIfHasError();
        return callResult.Data;
    }

    public async Task<IEnumerable<BybitKline>> GetCompletedCandlesticksAsync(string symbol, KlineInterval timeframe)
    {
        var candlesticks = await this.GetAllCandlesticksAsync(symbol, timeframe);
        return candlesticks.SkipLast(1);
    }
}
