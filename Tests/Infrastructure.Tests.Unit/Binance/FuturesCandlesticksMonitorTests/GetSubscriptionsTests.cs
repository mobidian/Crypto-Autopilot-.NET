using Infrastructure.Tests.Unit.Binance.FuturesCandlesticksMonitorTests.Base;

namespace Infrastructure.Tests.Unit.Binance.FuturesCandlesticksMonitorTests;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[Parallelizable(ParallelScope.All)]
public class GetSubscriptionsTests : FuturesCandlesticksMonitorTestsBase
{
    [Test]
    public void GetSubscriptions_ShouldReturnSubscriptions_WhenSubscriptionsExist()
    {
        // Arrange
        var contracts = this.GetRandomContractIdentifiers(Random.Shared.Next(10, 15));
        contracts.ForEach(contract => this.SubscriptionsDictionary.Add(contract, null!));

        // Act
        var returnedContracts = this.SUT.GetSubscriptions();

        // Assert
        returnedContracts.Should().BeEquivalentTo(contracts);
    }

    [Test]
    public async Task GetSubscriptions_ShouldNotContainKey_WhenUnsubscribedAfterBeeingSubscribed()
    {
        // Arrange
        var contracts = this.GetRandomContractIdentifiers(Random.Shared.Next(10, 15));
        var existingContract = this.Faker.PickRandom(contracts);

        await this.SUT.SubscribeToKlineUpdatesAsync(existingContract.currencyPair, existingContract.contractType, existingContract.timeframe);
        await this.SUT.UnsubscribeFromKlineUpdatesAsync(existingContract.currencyPair, existingContract.contractType, existingContract.timeframe);


        // Act
        var returnedContracts = this.SUT.GetSubscriptions();


        // Assert
        returnedContracts.Should().NotContain((existingContract.currencyPair, existingContract.contractType, existingContract.timeframe));
    }

    [Test]
    public void GetSubscriptions_ShouldReturnEmptyEnumerable_WhenNoSubscriptionsExist()
    {
        // Act
        var returnedContracts = this.SUT.GetSubscriptions();

        // Assert
        returnedContracts.Should().BeEmpty();
    }
}
