using Infrastructure.Tests.Unit.FuturesCandlesticksMonitorTests.Base;

namespace Infrastructure.Tests.Unit.FuturesCandlesticksMonitorTests;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[Parallelizable(ParallelScope.All)]
public class IsSubscribedToTests : FuturesCandlesticksMonitorTestsBase
{
    [Test]
    public void IsSubscribedTo_ShouldReturnTrue_WhenSubscribed()
    {
        // Arrange
        var contracts = this.GetRandomContractIdentifiers(Random.Shared.Next(10, 15));
        var existingContract = this.Faker.PickRandom(contracts);
        contracts.ForEach(key => this.SubscriptionsDictionary.Add(key, null!));

        // Act
        var subscribed = this.SUT.IsSubscribedTo(existingContract.currencyPair, existingContract.contractType, existingContract.timeframe);

        // Assert
        subscribed.Should().BeTrue();
    }

    [Test]
    public async Task IsSubscribedTo_ShouldReturnFalse_WhenUnsubscribedAfterBeeingSubscribed()
    {
        // Arrange
        var contracts = this.GetRandomContractIdentifiers(Random.Shared.Next(10, 15));
        var existingContract = this.Faker.PickRandom(contracts);

        await this.SUT.SubscribeToKlineUpdatesAsync(existingContract.currencyPair, existingContract.contractType, existingContract.timeframe);
        await this.SUT.UnsubscribeFromKlineUpdatesAsync(existingContract.currencyPair, existingContract.contractType, existingContract.timeframe);


        // Act
        var subscribed = this.SUT.IsSubscribedTo(existingContract.currencyPair, existingContract.contractType, existingContract.timeframe);


        // Assert
        subscribed.Should().BeFalse();
    }

    [Test]
    public void IsSubscribedTo_ShouldReturnFalse_WhenNotSubscribed()
    {
        // Arrange
        var nonExistingContract = this.GetRandomContractIdentifier();
        
        // Act
        var subscribed = this.SUT.IsSubscribedTo(nonExistingContract.currencyPair, nonExistingContract.contractType, nonExistingContract.timeframe);
        
        // Assert
        subscribed.Should().BeFalse();
    }
}
