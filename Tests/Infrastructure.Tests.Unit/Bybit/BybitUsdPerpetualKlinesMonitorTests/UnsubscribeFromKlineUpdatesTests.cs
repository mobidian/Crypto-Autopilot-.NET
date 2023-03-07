using Infrastructure.Tests.Unit.Bybit.BybitUsdPerpetualKlinesMonitorTests.AbstractBase;

namespace Infrastructure.Tests.Unit.Bybit.BybitUsdPerpetualKlinesMonitorTests;

public class UnsubscribeFromKlineUpdatesTests : BybitUsdPerpetualKlinesMonitorTestsBase
{
    [Test]
    public async Task UnsubscribeFromKlineUpdatesAsync_ShouldUnsubscribesFromKlineUpdates_WhenSubscribed()
    {
        // Arrange
        var contracts = this.GetRandomContractIdentifiers(Random.Shared.Next(10, 15));
        var contractToRemove = this.Faker.PickRandom(contracts);
        await this.FuturesStreamsSubscribeToAllContractsAsync(contracts);

        // Act
        await this.SUT.UnsubscribeFromKlineUpdatesAsync(contractToRemove.currencyPair, contractToRemove.timeframe);

        // Assert
        await this.FuturesStreams.UnsubscribeAsync(Arg.Is<int>(id => id == this.Subscription.Id));
        this.SubscriptionsDictionary.Should().NotContainKey(contractToRemove);
        this.SubscriptionsDictionary.Should().ContainKeys(contracts.Except(new[] { contractToRemove }));
    }

    [Test]
    public async Task UnsubscribeFromKlineUpdatesAsync_ShouldThrow_WhenNotSubscribed()
    {
        // Arrange
        var contracts = this.GetRandomContractIdentifiers(Random.Shared.Next(10, 15));
        var contractToRemove = this.GetRandomContractIdentifierExcept(contracts);
        await this.FuturesStreamsSubscribeToAllContractsAsync(contracts);


        // Act
        var func = async () => await this.SUT.UnsubscribeFromKlineUpdatesAsync(contractToRemove.currencyPair, contractToRemove.timeframe);


        // Assert
        await this.FuturesStreams.DidNotReceive().UnsubscribeAsync(Arg.Any<int>());

        await func.Should().ThrowExactlyAsync<KeyNotFoundException>().WithMessage($"The given contract identifier (currencyPair = {contractToRemove.currencyPair}, timeframe = {contractToRemove.timeframe}) was not present in the subscriptions dictionary.");
        this.SubscriptionsDictionary.Should().ContainKeys(contracts);
    }
}
