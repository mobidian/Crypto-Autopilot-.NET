using Application.Interfaces.Proxies;
using Application.Interfaces.Services.General;
using Application.Interfaces.Services.Trading.BybitExchange.Monitors;

using Bybit.Net.Enums;
using Bybit.Net.Interfaces.Clients.UsdPerpetualApi;
using Bybit.Net.Objects.Models.Socket;

using CryptoExchange.Net.Sockets;

using Infrastructure.Extensions;

namespace Infrastructure.Services.Trading.BybitExchange.Monitors;

public class BybitUsdPerpetualKlinesMonitor : IBybitUsdPerpetualKlinesMonitor
{
    private readonly IBybitSocketClientUsdPerpetualStreams FuturesStreams;
    private readonly IDateTimeProvider DateTimeProvider;
    private readonly Func<IUpdateSubscriptionProxy> SubscriptionFactory;
    
    public BybitUsdPerpetualKlinesMonitor(IBybitSocketClientUsdPerpetualStreams futuresStreams, IDateTimeProvider dateTimeProvider, Func<IUpdateSubscriptionProxy> subscriptionFactory)
    {
        this.FuturesStreams = futuresStreams ?? throw new ArgumentNullException(nameof(futuresStreams));
        this.DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        this.SubscriptionFactory = subscriptionFactory ?? throw new ArgumentNullException(nameof(subscriptionFactory));
        
        this.DataDictionary = new Dictionary<(string currencyPair, KlineInterval timeframe), BybitKlineUpdate?>();
        this.SubscriptionsDictionary = new Dictionary<(string currencyPair, KlineInterval timeframe), IUpdateSubscriptionProxy>();
    }

    //// //// ////
    
    internal readonly IDictionary<(string currencyPair, KlineInterval timeframe), BybitKlineUpdate?> DataDictionary;
    internal readonly IDictionary<(string currencyPair, KlineInterval timeframe), IUpdateSubscriptionProxy> SubscriptionsDictionary;

    public async Task SubscribeToKlineUpdatesAsync(string currencyPair, KlineInterval timeframe)
    {
        var contractIdentifier = (currencyPair, timeframe);

        if (this.SubscriptionsDictionary.ContainsKey(contractIdentifier))
            return;
        

        var callResult = await this.FuturesStreams.SubscribeToKlineUpdatesAsync(currencyPair, timeframe, HandleKlineUpdate);
        callResult.ThrowIfHasError();

        var subscription = this.SubscriptionFactory.Invoke();
        subscription.SetSubscription(callResult.Data);
        this.SubscriptionsDictionary[contractIdentifier] = subscription;
    }
    private void HandleKlineUpdate(DataEvent<IEnumerable<BybitKlineUpdate>> dataEvent)
    {
        var strings = dataEvent.Topic!.Split('.');
        var currencyPair = strings.Last();
        var timeframe = (KlineInterval)(Convert.ToInt32(strings.First()) * 60);
        
        var lastCandlestick = dataEvent.Data.Last();
        this.DataDictionary[(currencyPair, timeframe)] = lastCandlestick;
    }

    public bool IsSubscribedTo(string currencyPair, KlineInterval timeframe) => this.SubscriptionsDictionary.ContainsKey((currencyPair, timeframe));

    public async Task UnsubscribeFromKlineUpdatesAsync(string currencyPair, KlineInterval timeframe)
    {
        var contractIdentifier = (currencyPair, timeframe);

        var subscription = this.SubscriptionsDictionary[contractIdentifier];
        await this.FuturesStreams.UnsubscribeAsync(subscription.Id);
        
        this.SubscriptionsDictionary.Remove(contractIdentifier);
    }

    public async Task<BybitKlineUpdate> WaitForNextCandlestickAsync(string currencyPair, KlineInterval timeframe)
    {
        var contractIdentifier = (currencyPair, timeframe);

        if (!this.SubscriptionsDictionary.ContainsKey(contractIdentifier))
            throw new KeyNotFoundException($"The given contract identifier ({nameof(currencyPair)} = {currencyPair}, {nameof(timeframe)} = {timeframe}) was not present in the subscriptions dictionary.");

        var invokeTimeUtc = this.DateTimeProvider.UtcNow;

        // adds null value in case this method was called before any update has been received
        this.DataDictionary.TryAdd(contractIdentifier, null);
        
        // waits for the first data event update, updating the value
        while (this.DataDictionary[contractIdentifier] is null)
            await Task.Delay(20);

        // waits for the new candlestick to be created
        while (this.DataDictionary[contractIdentifier]?.OpenTime <= invokeTimeUtc)
            await Task.Delay(20);

        return this.DataDictionary[contractIdentifier]!;
    }
}
