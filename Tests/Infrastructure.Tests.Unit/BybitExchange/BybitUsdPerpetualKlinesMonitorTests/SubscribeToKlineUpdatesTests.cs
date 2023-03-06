using Infrastructure.Tests.Unit.BybitExchange.BybitUsdPerpetualKlinesMonitorTests.AbstractBase;

namespace Infrastructure.Tests.Unit.BybitExchange.BybitUsdPerpetualKlinesMonitorTests;

public class SubscribeToKlineUpdatesTests : BybitUsdPerpetualKlinesMonitorTestsBase
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
        this.SubscriptionsDictionary.Keys.AsEnumerable().Should().BeEquivalentTo(contracts);
    }

    [Test]
    public async Task SubscribeToKlineUpdatesAsync_ShouldNotSubscribeToKlineUpdates_WhenAlreadySubscribed()
    {
        // Arrange
        var contracts = this.GetRandomContractIdentifiers(Random.Shared.Next(10, 15));
        var existingContract = this.Faker.PickRandom(contracts);

        await FuturesStreamsSubscribeToAllContractsAsync(contracts);


        // Act
        await this.SUT.SubscribeToKlineUpdatesAsync(existingContract.currencyPair, existingContract.timeframe);

        
        // Assert
        this.FuturesStreams.ReceivedCalls().Count().Should().Be(contracts.Count);
        await FuturesStreamsReceivedSubscribeCallsForEveryContractAssertionAsync(contracts);

        this.SubscriptionsDictionary.Keys.AsEnumerable().Should().BeEquivalentTo(contracts);
    }
}
