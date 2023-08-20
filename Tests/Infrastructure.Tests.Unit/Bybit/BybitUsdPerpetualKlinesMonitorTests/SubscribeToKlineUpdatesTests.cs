using FluentAssertions;

using Infrastructure.Tests.Unit.Bybit.BybitUsdPerpetualKlinesMonitorTests.AbstractBase;

using NSubstitute;

using Xunit;

namespace Infrastructure.Tests.Unit.Bybit.BybitUsdPerpetualKlinesMonitorTests;

public class SubscribeToKlineUpdatesTests : BybitUsdPerpetualKlinesMonitorTestsBase
{
    [Fact]
    public async Task SubscribeToKlineUpdatesAsync_ShouldSubscribeToKlineUpdates_WhenNotSubscribed()
    {
        // Arrange
        var contracts = this.GetRandomContractIdentifiers(Random.Shared.Next(10, 15));

        // Act
        await this.FuturesStreamsSubscribeToAllContractsAsync(contracts);

        // Assert
        await this.FuturesStreamsReceivedSubscribeCallsForEveryContractAssertionAsync(contracts);
        this.SubscriptionsDictionary.Keys.AsEnumerable().Should().BeEquivalentTo(contracts);
    }

    [Fact]
    public async Task SubscribeToKlineUpdatesAsync_ShouldNotSubscribeToKlineUpdates_WhenAlreadySubscribed()
    {
        // Arrange
        var contracts = this.GetRandomContractIdentifiers(Random.Shared.Next(10, 15));
        var existingContract = this.Faker.PickRandom(contracts);

        await this.FuturesStreamsSubscribeToAllContractsAsync(contracts);


        // Act
        await this.SUT.SubscribeToKlineUpdatesAsync(existingContract.currencyPair, existingContract.timeframe);


        // Assert
        this.FuturesStreams.ReceivedCalls().Count().Should().Be(contracts.Count);
        await this.FuturesStreamsReceivedSubscribeCallsForEveryContractAssertionAsync(contracts);

        this.SubscriptionsDictionary.Keys.AsEnumerable().Should().BeEquivalentTo(contracts);
    }
}
