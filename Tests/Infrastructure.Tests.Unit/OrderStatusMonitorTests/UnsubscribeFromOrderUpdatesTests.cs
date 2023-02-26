using Application.Interfaces.Proxies;

using CryptoExchange.Net.Sockets;

using Infrastructure.Tests.Unit.OrderStatusMonitorTests.Base;

namespace Infrastructure.Tests.Unit.OrderStatusMonitorTests;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[Parallelizable(ParallelScope.All)]
public class UnsubscribeFromOrderUpdatesTests : OrderStatusMonitorTestsBase
{
    [Test]
    public async Task UnsubscribeFromOrderUpdatesAsync_ShouldUnsubscribeFromOrderUpdates_WhenSubscribed()
    {
        // Arrange
        await this.SUT.SubscribeToOrderUpdatesAsync();

        // Act
        await this.SUT.UnsubscribeFromOrderUpdatesAsync();

        // Assert
        await this.UserDataUpdatesSubscription.Received(1).CloseAsync();
        this.SUT.Subscribed.Should().BeFalse();
    }
}