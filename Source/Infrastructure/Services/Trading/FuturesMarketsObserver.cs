using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Interfaces.Clients.UsdFuturesApi;

using CryptoExchange.Net.Sockets;

using Domain.Models;

using Infrastructure.Common;

namespace Infrastructure.Services.Trading;

public class FuturesMarketsObserver
{
    public CurrencyPair CurrencyPair { get; }
    public KlineInterval Timeframe { get; }

    private readonly IBinanceSocketClientUsdFuturesStreams FuturesStreams;

    public FuturesMarketsObserver(CurrencyPair currencyPair, KlineInterval timeframe, IBinanceSocketClientUsdFuturesStreams futuresStreams)
    {
        this.CurrencyPair = currencyPair;
        this.Timeframe = timeframe;
        this.FuturesStreams = futuresStreams;
    }

    
    //// //// ////
    
    private UpdateSubscription KlineUpdatesSubscription = default!;
    private DateTime? OpenTime = null;
    private IBinanceStreamKlineData StreamKlineData = default!;
    public bool SubscribedToKlineUpdates { get; private set; } = false;

    public async Task SubscribeToKlineUpdatesAsync()
    {
        var callResult = await this.FuturesStreams.SubscribeToKlineUpdatesAsync(this.CurrencyPair.Name, this.Timeframe, HandleKlineUpdate);
        callResult.ThrowIfHasError("Could not subscribe to kline updates");

        this.KlineUpdatesSubscription = callResult.Data;

        this.SubscribedToKlineUpdates = true;
    }
    internal void HandleKlineUpdate(DataEvent<IBinanceStreamKlineData> dataEvent)
    {
        var latestOpenTime = dataEvent.Data.Data.OpenTime;

        this.OpenTime ??= latestOpenTime;
        
        UpdateStreamKlineDataIfNewCandlestick(dataEvent, latestOpenTime);
    }
    private void UpdateStreamKlineDataIfNewCandlestick(DataEvent<IBinanceStreamKlineData> dataEvent, DateTime latestOpenTime)
    {
        if (this.OpenTime == latestOpenTime)
            return;

        this.StreamKlineData = dataEvent.Data;
    }

    public async Task UnsubscribeFromKlineUpdatesAsync()
    {
        await this.KlineUpdatesSubscription.CloseAsync();
        this.SubscribedToKlineUpdates = false;
    }

    public async Task<IBinanceStreamKlineData> WaitForNewCandlestickAsync()
    {
        if (!this.SubscribedToKlineUpdates)
            throw new Exception("Not subscribed to kline updates");
        


        var initialOpenTime = this.OpenTime;

        while (this.OpenTime == initialOpenTime)
        {
            await Task.Delay(10);
        }

        return this.StreamKlineData;
    }
}
