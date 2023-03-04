using Application.Interfaces.Logging;
using Application.Interfaces.Proxies;
using Application.Interfaces.Services.General;

using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Interfaces.Clients.UsdFuturesApi;
using Binance.Net.Objects.Models.Futures.Socket;

using CryptoExchange.Net.Objects;

using CryptoExchange.Net.Sockets;

using Infrastructure.Services.Trading.Binance.Monitors;

namespace Infrastructure.Tests.Unit.Binance.FuturesCandlesticksMonitorTests.Base;

public abstract class FuturesCandlesticksMonitorTestsBase
{
    protected readonly FuturesCandlesticksMonitor SUT;
    protected readonly IBinanceSocketClientUsdFuturesStreams FuturesStreams = Substitute.For<IBinanceSocketClientUsdFuturesStreams>();
    protected readonly IDateTimeProvider DateTimeProvider = Substitute.For<IDateTimeProvider>();
    protected readonly Func<IUpdateSubscriptionProxy> SubscriptionFactory = Substitute.For<Func<IUpdateSubscriptionProxy>>();
    protected readonly ILoggerAdapter<FuturesCandlesticksMonitor> Logger = Substitute.For<ILoggerAdapter<FuturesCandlesticksMonitor>>();
    protected readonly IDictionary<(string currencyPair, ContractType contractType, KlineInterval timeframe), BinanceStreamContinuousKlineData?> DataDictionary;
    protected readonly IDictionary<(string currencyPair, ContractType contractType, KlineInterval timeframe), IUpdateSubscriptionProxy> SubscriptionsDictionary;

    protected readonly IUpdateSubscriptionProxy Subscription = Substitute.For<IUpdateSubscriptionProxy>();

    public FuturesCandlesticksMonitorTestsBase()
    {
        this.DataDictionary = new Dictionary<(string currencyPair, ContractType contractType, KlineInterval timeframe), BinanceStreamContinuousKlineData?>();
        this.SubscriptionsDictionary = new Dictionary<(string currencyPair, ContractType contractType, KlineInterval timeframe), IUpdateSubscriptionProxy>();


        this.SubscriptionFactory.Invoke().Returns(this.Subscription);

        var updateSubscriptionCallResult = new CallResult<UpdateSubscription>(new UpdateSubscription(null!, null!));
        this.FuturesStreams
            .SubscribeToContinuousContractKlineUpdatesAsync(
                Arg.Any<string>(),
                Arg.Any<ContractType>(),
                Arg.Any<KlineInterval>(),
                Arg.Any<Action<DataEvent<BinanceStreamContinuousKlineData>>>())
            .Returns(Task.FromResult(updateSubscriptionCallResult));

        this.SUT = new FuturesCandlesticksMonitor(this.FuturesStreams, this.DateTimeProvider, this.SubscriptionFactory, this.Logger, this.DataDictionary, this.SubscriptionsDictionary);
    }


    //// //// //// ////


    protected readonly Faker Faker = new Faker();
    protected readonly Faker<string> CurrencyPairFaker = new Faker<string>()
        .CustomInstantiator(f => $"{f.Finance.Currency().Code}{f.Finance.Currency().Code}");

    protected (string currencyPair, ContractType contractType, KlineInterval timeframe) GetRandomContractIdentifier()
    {
        var currencyPair = this.CurrencyPairFaker.Generate();
        var contractType = this.Faker.PickRandom<ContractType>();
        var timeframe = this.Faker.PickRandom<KlineInterval>();
        return (currencyPair, contractType, timeframe);
    }
    protected (string currencyPair, ContractType contractType, KlineInterval timeframe) GetRandomContractIdentifierExcept(IEnumerable<(string currencyPair, ContractType contractType, KlineInterval timeframe)> values)
    {
        (string currencyPair, ContractType contractType, KlineInterval timeframe) contractIdentifier;
        do
        {
            contractIdentifier = this.GetRandomContractIdentifier();
        } while (values.Contains(contractIdentifier));

        return contractIdentifier;
    }
    protected List<(string currencyPair, ContractType contractType, KlineInterval timeframe)> GetRandomContractIdentifiers(int n)
    {
        var list = new List<(string currencyPair, ContractType contractType, KlineInterval timeframe)>();

        for (var i = 0; i < n; i++)
            list.Add(this.GetRandomContractIdentifier());

        return list;
    }


    protected async Task FuturesStreamsSubscribeToAllContractsAsync(List<(string currencyPair, ContractType contractType, KlineInterval timeframe)> contractIdentifiers)
    {
        for (var i = 0; i < contractIdentifiers.Count; i++)
            await this.SUT.SubscribeToKlineUpdatesAsync(contractIdentifiers[i].currencyPair, contractIdentifiers[i].contractType, contractIdentifiers[i].timeframe);
    }

    protected async Task FuturesStreamsReceivedSubscribeCallsForEveryContractAssertionAsync(List<(string currencyPair, ContractType contractType, KlineInterval timeframe)> contractIdentifiers)
    {
        for (var i = 0; i < contractIdentifiers.Count; i++)
            await this.FuturesStreams.Received(1).SubscribeToContinuousContractKlineUpdatesAsync(contractIdentifiers[i].currencyPair, contractIdentifiers[i].contractType, contractIdentifiers[i].timeframe, Arg.Any<Action<DataEvent<BinanceStreamContinuousKlineData>>>());
    }
}
