﻿using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Interfaces.Clients.UsdFuturesApi;

using CryptoExchange.Net.Sockets;

using Domain.Models;

using Infrastructure.Common;

namespace Infrastructure.Services.Trading;

public class FuturesMarketsCandlestickAwaiter : IFuturesMarketsCandlestickAwaiter
{
    public CurrencyPair CurrencyPair { get; }
    public KlineInterval Timeframe { get; }

    private readonly IBinanceSocketClientUsdFuturesStreams FuturesStreams;

    public FuturesMarketsCandlestickAwaiter(CurrencyPair currencyPair, KlineInterval timeframe, IBinanceSocketClientUsdFuturesStreams futuresStreams)
    {
        this.CurrencyPair = currencyPair;
        this.Timeframe = timeframe;
        this.FuturesStreams = futuresStreams;
    }

    //// //// ////

    private UpdateSubscription KlineUpdatesSubscription = default!;
    private DateTime CurrentOpenTime = DateTime.MinValue;
    private IBinanceStreamKlineData StreamKlineData = default!;
    public bool SubscribedToKlineUpdates { get; private set; } = false;

    public async Task SubscribeToKlineUpdatesAsync()
    {
        var callResult = await this.FuturesStreams.SubscribeToKlineUpdatesAsync(this.CurrencyPair.Name, this.Timeframe, HandleKlineUpdate);
        callResult.ThrowIfHasError("Could not subscribe to kline updates");
        
        this.WaitForFirstKlineUpdate();
        
        this.KlineUpdatesSubscription = callResult.Data;
        this.KlineUpdatesSubscription.ConnectionLost += () => throw new Exception();
        this.SubscribedToKlineUpdates = true;
    }
    private void WaitForFirstKlineUpdate()
    {
        while (this.CurrentOpenTime == DateTime.MinValue)
            continue;
    }
    internal void HandleKlineUpdate(DataEvent<IBinanceStreamKlineData> dataEvent)
    {
        var latestOpenTime = dataEvent.Data.Data.OpenTime;

        if (this.CurrentOpenTime == DateTime.MinValue)
            this.CurrentOpenTime = latestOpenTime;

        this.UpdateStreamKlineDataIfNewCandlestick(dataEvent, latestOpenTime);
    }
    private void UpdateStreamKlineDataIfNewCandlestick(DataEvent<IBinanceStreamKlineData> dataEvent, DateTime latestOpenTime)
    {
        if (this.CurrentOpenTime == latestOpenTime)
            return;
        
        this.CurrentOpenTime = latestOpenTime;
        this.StreamKlineData = dataEvent.Data;
    }

    public async Task UnsubscribeFromKlineUpdatesAsync()
    {
        await this.KlineUpdatesSubscription.CloseAsync();
        this.SubscribedToKlineUpdates = false;
    }

    public async Task<IBinanceStreamKlineData> WaitForNextCandlestickAsync()
    {
        if (!this.SubscribedToKlineUpdates)
            throw new Exception("Not subscribed to kline updates");
        
        var initialLatestOpenTime = this.CurrentOpenTime;
        while (this.CurrentOpenTime == initialLatestOpenTime)
        {
            await Task.Delay(10);
        }

        return this.StreamKlineData;
    }
}