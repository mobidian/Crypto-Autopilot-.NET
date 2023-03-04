using Infrastructure.Tests.Unit.Binance.FuturesCandlesticksMonitorTests.Base;

namespace Infrastructure.Tests.Unit.Binance.FuturesCandlesticksMonitorTests;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[Parallelizable(ParallelScope.All)]
public class SubscribeToKlineUpdatesTests : FuturesCandlesticksMonitorTestsBase
{
    [Test]
    public async Task SubscribeToKlineUpdatesAsync_ShouldSubscribeToKlineUpdates_WhenNotSubscribed()
    {
        // Arrange
        var contracts = this.GetRandomContractIdentifiers(Random.Shared.Next(10, 15));

        // Act
        await FuturesStreamsSubscribeToAllContractsAsync(contracts);

        // Assert
        await FuturesStreamsReceivedSubscribeCallsForEveryContractAssertionAsync(contracts);
        this.SUT.SubscriptionsDictionary.Keys.AsEnumerable().Should().BeEquivalentTo(contracts);
    }

    [Test]
    public async Task SubscribeToKlineUpdatesAsync_ShouldNotSubscribeToKlineUpdates_WhenAlreadySubscribed()
    {
        // Arrange
        var contracts = this.GetRandomContractIdentifiers(Random.Shared.Next(10, 15));
        var existingContract = this.Faker.PickRandom(contracts);

        await FuturesStreamsSubscribeToAllContractsAsync(contracts);


        // Act
        await this.SUT.SubscribeToKlineUpdatesAsync(existingContract.currencyPair, existingContract.contractType, existingContract.timeframe);


        // Assert
        this.FuturesStreams.ReceivedCalls().Count().Should().Be(contracts.Count);
        await FuturesStreamsReceivedSubscribeCallsForEveryContractAssertionAsync(contracts);

        this.SUT.SubscriptionsDictionary.Keys.AsEnumerable().Should().BeEquivalentTo(contracts);
    }
}
