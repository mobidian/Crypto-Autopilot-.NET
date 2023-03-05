using Application.Interfaces.Services.General;
using Application.Interfaces.Services.Trading.BybitExchange;

using Bybit.Net.Enums;
using Bybit.Net.Interfaces.Clients.UsdPerpetualApi;
using Bybit.Net.Objects.Models;

using Infrastructure.Extensions;

namespace Infrastructure.Services.Trading.BybitExchange;

public class BybitUsdFuturesDataProvider : IBybitUsdFuturesDataProvider
{
    private readonly IDateTimeProvider DateTime;
    private readonly IBybitClientUsdPerpetualApiExchangeData FuturesExchangeData;
    
    public BybitUsdFuturesDataProvider(IDateTimeProvider dateTime, IBybitClientUsdPerpetualApiExchangeData futuresExchangeData)
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

    public async Task<IEnumerable<BybitKline>> GetAllCandlesticksAsync(string symbol, KlineInterval timeframe, int limit = 1000)
    {
        var seconds = (int)timeframe * limit;
        var callResult = await this.FuturesExchangeData.GetKlinesAsync(symbol, timeframe, this.DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(seconds)));
        callResult.ThrowIfHasError();
        return callResult.Data;
    }
       
    public async Task<IEnumerable<BybitKline>> GetCompletedCandlesticksAsync(string symbol, KlineInterval timeframe, int limit = 1000)
    {
        var candlesticks = await this.GetCompletedCandlesticksAsync(symbol, timeframe, limit);
        return candlesticks.SkipLast(1);
    }
}
