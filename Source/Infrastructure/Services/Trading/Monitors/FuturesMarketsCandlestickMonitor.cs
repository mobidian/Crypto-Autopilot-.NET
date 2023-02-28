using Application.Interfaces.Logging;
using Application.Interfaces.Proxies;
using Application.Interfaces.Services.General;
using Application.Interfaces.Services.Trading.Monitors;

using Binance.Net.Enums;
using Binance.Net.Interfaces.Clients.UsdFuturesApi;
using Binance.Net.Objects.Models.Futures.Socket;

using CryptoExchange.Net.Sockets;

using Infrastructure.Extensions;

namespace Infrastructure.Services.Trading.Monitors;

public class FuturesCandlesticksMonitor : IFuturesCandlesticksMonitor
{
    private readonly IBinanceSocketClientUsdFuturesStreams FuturesStreams;
    private readonly IDateTimeProvider DateTimeProvider;
    private readonly Func<IUpdateSubscriptionProxy> SubscriptionFactory;
    private readonly ILoggerAdapter<FuturesCandlesticksMonitor> Logger;

    public FuturesCandlesticksMonitor(IBinanceSocketClientUsdFuturesStreams futuresStreams, IDateTimeProvider dateTimeProvider, Func<IUpdateSubscriptionProxy> subscriptionFactory, ILoggerAdapter<FuturesCandlesticksMonitor> logger)
    {
        this.FuturesStreams = futuresStreams;
        this.DateTimeProvider = dateTimeProvider;
        this.SubscriptionFactory = subscriptionFactory;
        this.Logger = logger;

        this.DataDictionary = new Dictionary<(string currencyPair, ContractType contractType, KlineInterval timeframe), BinanceStreamContinuousKlineData?>();
        this.SubscriptionsDictionary = new Dictionary<(string currencyPair, ContractType contractType, KlineInterval timeframe), IUpdateSubscriptionProxy>();
    }
    internal FuturesCandlesticksMonitor(IBinanceSocketClientUsdFuturesStreams futuresStreams, IDateTimeProvider dateTimeProvider, Func<IUpdateSubscriptionProxy> subscriptionFactory, ILoggerAdapter<FuturesCandlesticksMonitor> logger, IDictionary<(string currencyPair, ContractType contractType, KlineInterval timeframe), BinanceStreamContinuousKlineData?> dataDictionary, IDictionary<(string currencyPair, ContractType contractType, KlineInterval timeframe), IUpdateSubscriptionProxy> subscriptionsDictionary)
    {
        this.FuturesStreams = futuresStreams;
        this.DateTimeProvider = dateTimeProvider;
        this.SubscriptionFactory = subscriptionFactory;
        this.Logger = logger;

        this.DataDictionary = dataDictionary;
        this.SubscriptionsDictionary = subscriptionsDictionary;
    }


    internal readonly IDictionary<(string currencyPair, ContractType contractType, KlineInterval timeframe), BinanceStreamContinuousKlineData?> DataDictionary;
    internal readonly IDictionary<(string currencyPair, ContractType contractType, KlineInterval timeframe), IUpdateSubscriptionProxy> SubscriptionsDictionary;

    public async Task SubscribeToKlineUpdatesAsync(string currencyPair, ContractType contractType, KlineInterval timeframe)
    {
        var contractIdentifier = (currencyPair, contractType, timeframe);

        if (this.SubscriptionsDictionary.ContainsKey(contractIdentifier))
            return;


        var callResult = await this.FuturesStreams.SubscribeToContinuousContractKlineUpdatesAsync(currencyPair, contractType, timeframe, HandleContractKlineUpdate);
        callResult.ThrowIfHasError($"Could not subscribe to {currencyPair} {contractType} contract on the {timeframe} timeframe");


        var subscription = this.SubscriptionFactory.Invoke();
        subscription.SetSubscription(callResult.Data);
        subscription.ConnectionLost += async () => await Subscription_ConnectionLost(currencyPair, contractType, timeframe);
        subscription.ConnectionRestored += async disconnectedTime => await Subscription_ConnectionRestored(disconnectedTime, currencyPair, contractType, timeframe);

        this.SubscriptionsDictionary[contractIdentifier] = subscription;
    }
    internal void HandleContractKlineUpdate(DataEvent<BinanceStreamContinuousKlineData> dataEvent)
    {
        var streamKlineData = dataEvent.Data;
        var streamKline = streamKlineData.Data;

        var currencyPair = streamKlineData.Symbol;
        var contractType = streamKlineData.ContractType;
        var timeframe = streamKline.Interval;

        this.DataDictionary[(currencyPair, contractType, timeframe)] = streamKlineData;
    }
    private static async Task Subscription_ConnectionLost(string currencyPair, ContractType contractType, KlineInterval timeframe)
    {
        // // TODO Subscription_ConnectionLost implementation // //
        await Task.CompletedTask;
        throw new NotImplementedException();
    }
    private static async Task Subscription_ConnectionRestored(TimeSpan disconnectedTime, string currencyPair, ContractType contractType, KlineInterval timeframe)
    {
        // // TODO Subscription_ConnectionRestored implementation // //
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    public bool IsSubscribedTo(string currencyPair, ContractType contractType, KlineInterval timeframe) => this.SubscriptionsDictionary.ContainsKey((currencyPair, contractType, timeframe));
    public IEnumerable<(string currencyPair, ContractType contractType, KlineInterval timeframe)> GetSubscriptions() => this.SubscriptionsDictionary.Keys.AsEnumerable();

    public async Task UnsubscribeFromKlineUpdatesAsync(string currencyPair, ContractType contractType, KlineInterval timeframe)
    {
        var contractIdentifier = (currencyPair, contractType, timeframe);

        if (!this.SubscriptionsDictionary.TryGetValue(contractIdentifier, out var subscription))
            throw new KeyNotFoundException($"The given contract identifier ({nameof(currencyPair)} = {currencyPair}, {nameof(contractType)} = {contractType}, {nameof(timeframe)} = {timeframe}) was not present in the subscriptions dictionary.");

        await subscription.CloseAsync();
        this.SubscriptionsDictionary.Remove(contractIdentifier);
        this.DataDictionary.Remove(contractIdentifier);
    }

    public async Task<BinanceStreamContinuousKlineData> WaitForNextCandlestickAsync(string currencyPair, ContractType contractType, KlineInterval timeframe)
    {
        var contractIdentifier = (currencyPair, contractType, timeframe);

        if (!this.SubscriptionsDictionary.ContainsKey(contractIdentifier))
            throw new KeyNotFoundException($"The given contract identifier ({nameof(currencyPair)} = {currencyPair}, {nameof(contractType)} = {contractType}, {nameof(timeframe)} = {timeframe}) was not present in the subscriptions dictionary.");


        var invokeTimeUtc = this.DateTimeProvider.UtcNow;

        // adds null value in case this method was called before any update has been received
        this.DataDictionary.TryAdd(contractIdentifier, null);

        // waits for the first data event update, updating the value
        while (this.DataDictionary[contractIdentifier] is null)
            await Task.Delay(20);

        // waits for the new candlestick to be created
        while (this.DataDictionary[contractIdentifier]?.Data.OpenTime <= invokeTimeUtc)
            await Task.Delay(20);

        return this.DataDictionary[contractIdentifier]!;

        // // TODO optimization (ex: CollectionsMarshal.GetValueRefOrNullRef(...)) // //
    }


    //// //// ////


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

        if (disposing)
            this.FuturesStreams.Dispose();

        this.DataDictionary.Clear();
        this.SubscriptionsDictionary.Clear();

        this.Disposed = true;
    }
}
