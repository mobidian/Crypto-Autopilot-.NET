using Bybit.Net.Objects.Models.Socket;

using CryptoExchange.Net.Sockets;

using FluentAssertions;

using Infrastructure.Tests.Unit.Bybit.ByBitUsdPerpetualOrderMonitorTests.AbstractBase;

using NSubstitute;

using Xunit;

namespace Infrastructure.Tests.Unit.Bybit.ByBitUsdPerpetualOrderMonitorTests;

public class SubscribeToOrderUpdatesTests : ByBitUsdPerpetualOrderMonitorTestsBase
{
    [Fact]
    public async Task SubscribeToOrderUpdatesAsync_ShouldSubscribeToOrderUpdates_WhenNotSubscribed()
    {
        // Act
        await this.SUT.SubscribeToOrderUpdatesAsync();
        
        // Assert
        this.SUT.Subscribed.Should().BeTrue();
        await this.UsdPerpetualStreams.Received(1).SubscribeToOrderUpdatesAsync(Arg.Any<Action<DataEvent<IEnumerable<BybitUsdPerpetualOrderUpdate>>>>());
        this.UsdPerpetualUpdatesSubscription.Received(1).SetSubscription(Arg.Is(this.UpdateSubscriptionCallResult.Data));
    }
}
