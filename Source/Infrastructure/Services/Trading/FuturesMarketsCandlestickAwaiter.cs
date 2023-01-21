using Application.Interfaces.Logging;
using Application.Interfaces.Proxies;

using Binance.Net.Enums;
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
    private IUpdateSubscriptionProxy KlineUpdatesSubscription;
    private readonly ILoggerAdapter<FuturesMarketsCandlestickAwaiter> Logger;

    public FuturesMarketsCandlestickAwaiter(CurrencyPair currencyPair, KlineInterval timeframe, IBinanceSocketClientUsdFuturesStreams futuresStreams, IUpdateSubscriptionProxy klineUpdatesSubscription, ILoggerAdapter<FuturesMarketsCandlestickAwaiter> logger)
    {
        this.CurrencyPair = currencyPair;
        this.Timeframe = timeframe;
        this.FuturesStreams = futuresStreams;
        this.KlineUpdatesSubscription = klineUpdatesSubscription;
        this.Logger = logger;
    }

    //// //// ////

    private DateTime CurrentOpenTime = DateTime.MinValue;
    private IBinanceStreamKlineData StreamKlineData = default!;
    public bool SubscribedToKlineUpdates { get; private set; } = false;

    public async Task SubscribeToKlineUpdatesAsync()
    {
        var callResult = await this.FuturesStreams.SubscribeToKlineUpdatesAsync(this.CurrencyPair.Name, this.Timeframe, HandleKlineUpdate);
        callResult.ThrowIfHasError("Could not subscribe to kline updates");

        this.WaitForFirstKlineUpdate();

        this.KlineUpdatesSubscription.SetSubscription(callResult.Data);
        this.KlineUpdatesSubscription.ConnectionLost += this.HandleKlineUpdatesSubscriptionConnectionLostAsync;
        this.SubscribedToKlineUpdates = true;
    }
    private async void HandleKlineUpdatesSubscriptionConnectionLostAsync()
    {
        this.Logger.LogInformation("Connection to {0} has been lost, attempting to reconnect", nameof(this.KlineUpdatesSubscription));
        await this.KlineUpdatesSubscription.ReconnectAsync();
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

    //// //// ////

    private bool Disposed;

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (this.Disposed)
            return;


        if (disposing)
            this.FuturesStreams.Dispose();

        this.KlineUpdatesSubscription = null!;
        this.StreamKlineData = null!;
        
        this.Disposed = true;
    }
}
