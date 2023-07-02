using Application.Interfaces.Proxies;
using Application.Interfaces.Services.General;

using Bybit.Net.Enums;
using Bybit.Net.Interfaces.Clients.UsdPerpetualApi;
using Bybit.Net.Objects.Models.Socket;

using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;

using Infrastructure.Services.Bybit.Monitors;

using NSubstitute;

namespace Infrastructure.Tests.Unit.Bybit.BybitUsdPerpetualKlinesMonitorTests.AbstractBase;

public abstract class BybitUsdPerpetualKlinesMonitorTestsBase : FakersClass
{
    protected readonly BybitUsdPerpetualKlinesMonitor SUT;

    protected readonly IBybitSocketClientUsdPerpetualApi FuturesStreams = Substitute.For<IBybitSocketClientUsdPerpetualApi>();
    protected readonly IDateTimeProvider DateTimeProvider = Substitute.For<IDateTimeProvider>();
    protected readonly Func<IUpdateSubscriptionProxy> SubscriptionFactory = Substitute.For<Func<IUpdateSubscriptionProxy>>();
    protected readonly IDictionary<(string currencyPair, KlineInterval timeframe), BybitKlineUpdate?> DataDictionary = new Dictionary<(string currencyPair, KlineInterval timeframe), BybitKlineUpdate?>();
    protected readonly IDictionary<(string currencyPair, KlineInterval timeframe), IUpdateSubscriptionProxy> SubscriptionsDictionary = new Dictionary<(string currencyPair, KlineInterval timeframe), IUpdateSubscriptionProxy>();

    protected readonly CallResult<UpdateSubscription> UpdateSubscriptionCallResult = new(new UpdateSubscription(null!, null!));
    protected readonly IUpdateSubscriptionProxy Subscription = Substitute.For<IUpdateSubscriptionProxy>();

    public BybitUsdPerpetualKlinesMonitorTestsBase()
    {
        var id = Random.Shared.Next();
        this.Subscription.Id.Returns(id);
        this.SubscriptionFactory.Invoke().Returns(this.Subscription);
        this.FuturesStreams.SubscribeToKlineUpdatesAsync(Arg.Any<string>(), Arg.Any<KlineInterval>(), Arg.Any<Action<DataEvent<IEnumerable<BybitKlineUpdate>>>>()).Returns(Task.FromResult(this.UpdateSubscriptionCallResult));

        this.SUT = new BybitUsdPerpetualKlinesMonitor(this.FuturesStreams, this.DateTimeProvider, this.SubscriptionFactory, this.DataDictionary, this.SubscriptionsDictionary);
    }


    //// //// //// ////


    protected async Task FuturesStreamsSubscribeToAllContractsAsync(List<(string currencyPair, KlineInterval timeframe)> contractIdentifiers)
    {
        for (var i = 0; i < contractIdentifiers.Count; i++)
            await this.SUT.SubscribeToKlineUpdatesAsync(contractIdentifiers[i].currencyPair, contractIdentifiers[i].timeframe);
    }
    protected async Task FuturesStreamsReceivedSubscribeCallsForEveryContractAssertionAsync(List<(string currencyPair, KlineInterval timeframe)> contractIdentifiers)
    {
        for (var i = 0; i < contractIdentifiers.Count; i++)
            await this.FuturesStreams.Received(1).SubscribeToKlineUpdatesAsync(contractIdentifiers[i].currencyPair, contractIdentifiers[i].timeframe, Arg.Any<Action<DataEvent<IEnumerable<BybitKlineUpdate>>>>());
    }
}
