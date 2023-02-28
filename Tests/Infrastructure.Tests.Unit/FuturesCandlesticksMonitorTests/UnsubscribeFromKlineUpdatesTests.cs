using Infrastructure.Tests.Unit.FuturesCandlesticksMonitorTests.Base;

namespace Infrastructure.Tests.Unit.FuturesCandlesticksMonitorTests;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[Parallelizable(ParallelScope.All)]
public class UnsubscribeFromKlineUpdatesTests : FuturesCandlesticksMonitorTestsBase
{
    [Test]
    public async Task UnsubscribeFromKlineUpdatesAsync_ShouldUnsubscribesFromKlineUpdates_WhenSubscribed()
    {
        // Arrange
        var contracts = this.GetRandomContractIdentifiers(Random.Shared.Next(10, 15));
        var contractToRemove = this.Faker.PickRandom(contracts);
        await this.FuturesStreamsSubscribeToAllContractsAsync(contracts);

        // Act
        await this.SUT.UnsubscribeFromKlineUpdatesAsync(contractToRemove.currencyPair, contractToRemove.contractType ,contractToRemove.timeframe);
        
        // Assert
        await this.Subscription.Received(1).CloseAsync();
        this.SUT.SubscriptionsDictionary.Should().NotContainKey(contractToRemove);
        this.SUT.SubscriptionsDictionary.Should().ContainKeys(contracts.Except(new[] { contractToRemove }));
    }
    
    [Test]
    public async Task UnsubscribeFromKlineUpdatesAsync_ShouldThrow_WhenNotSubscribed()
    {
        // Arrange
        var contracts = this.GetRandomContractIdentifiers(Random.Shared.Next(10, 15));
        var contractToRemove = this.GetRandomContractIdentifierExcept(contracts);
        await this.FuturesStreamsSubscribeToAllContractsAsync(contracts);


        // Act
        var func = async () => await this.SUT.UnsubscribeFromKlineUpdatesAsync(contractToRemove.currencyPair, contractToRemove.contractType, contractToRemove.timeframe);


        // Assert
        await this.Subscription.DidNotReceive().CloseAsync();

        await func.Should().ThrowExactlyAsync<KeyNotFoundException>().WithMessage($"The given contract identifier (currencyPair = {contractToRemove.currencyPair}, contractType = {contractToRemove.contractType}, timeframe = {contractToRemove.timeframe}) was not present in the subscriptions dictionary.");
        this.SUT.SubscriptionsDictionary.Should().ContainKeys(contracts);
    }
}
