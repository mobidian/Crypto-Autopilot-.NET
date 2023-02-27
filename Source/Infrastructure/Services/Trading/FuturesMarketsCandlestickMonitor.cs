using Application.Interfaces.Logging;
using Application.Interfaces.Proxies;
using Application.Interfaces.Services.General;
using Application.Interfaces.Services.Trading;

using Binance.Net.Enums;
using Binance.Net.Interfaces.Clients.UsdFuturesApi;
using Binance.Net.Objects.Models.Futures.Socket;

using CryptoExchange.Net.Sockets;

using Infrastructure.Extensions;

namespace Infrastructure.Services.Trading;

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
        if (this.SubscriptionsDictionary.ContainsKey((currencyPair, contractType, timeframe)))
            return;

        
        var callResult = await this.FuturesStreams.SubscribeToContinuousContractKlineUpdatesAsync(currencyPair, contractType, timeframe, HandleContractKlineUpdate);
        callResult.ThrowIfHasError($"Could not subscribe to {currencyPair} {contractType} contract on the {timeframe} timeframe");

        this.SubscriptionsDictionary[(currencyPair, contractType, timeframe)] = this.SubscriptionFactory.Invoke();
        this.SubscriptionsDictionary[(currencyPair, contractType, timeframe)].SetSubscription(callResult.Data);
        this.SubscriptionsDictionary[(currencyPair, contractType, timeframe)].ConnectionLost += async () => await Subscription_ConnectionLost(currencyPair, contractType, timeframe);
        this.SubscriptionsDictionary[(currencyPair, contractType, timeframe)].ConnectionRestored += async disconnectedTime => await Subscription_ConnectionRestored(disconnectedTime, currencyPair, contractType, timeframe);
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
    
    public IEnumerable<(string currencyPair, ContractType contractType, KlineInterval timeframe)> GetSubscriptions() => this.SubscriptionsDictionary.Keys.AsEnumerable();

    public async Task UnsubscribeFromKlineUpdatesAsync(string currencyPair, ContractType contractType, KlineInterval timeframe)
    {
        if (!this.SubscriptionsDictionary.TryGetValue((currencyPair, contractType, timeframe), out var subscription))
            throw new KeyNotFoundException($"The given key ({nameof(currencyPair)} = {currencyPair}, {nameof(contractType)} = {contractType}, {nameof(timeframe)} = {timeframe}) was not present in the subscriptions dictionary.");
        
        await subscription.CloseAsync();
        this.SubscriptionsDictionary.Remove((currencyPair, contractType, timeframe));
        this.DataDictionary.Remove((currencyPair, contractType, timeframe));
    }

    public async Task<BinanceStreamContinuousKlineData> WaitForNextCandlestickAsync(string currencyPair, ContractType contractType, KlineInterval timeframe)
    {
        if (!this.SubscriptionsDictionary.ContainsKey((currencyPair, contractType, timeframe)))
            throw new KeyNotFoundException($"The given key ({nameof(currencyPair)} = {currencyPair}, {nameof(contractType)} = {contractType}, {nameof(timeframe)} = {timeframe}) was not present in the subscriptions dictionary.");


        var currentTimeUtc = this.DateTimeProvider.UtcNow;
        
        // in case this method was called right after the SubscribeToKlineUpdatesAsync method
        // and no update has been received yet, a null value will be added
        this.DataDictionary.TryAdd((currencyPair, contractType, timeframe), null);
        
        while (this.DataDictionary[(currencyPair, contractType, timeframe)] is null)
            await Task.Delay(20);

        do
        {
             // //  TODO // //
        } while (true);


        return this.DataDictionary[(currencyPair, contractType, timeframe)]!;
    }





    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
