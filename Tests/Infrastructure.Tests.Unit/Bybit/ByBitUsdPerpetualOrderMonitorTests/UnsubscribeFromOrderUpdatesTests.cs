using Application.Exceptions;

using FluentAssertions;

using Infrastructure.Tests.Unit.Bybit.ByBitUsdPerpetualOrderMonitorTests.AbstractBase;

using NSubstitute;

using Xunit;

namespace Infrastructure.Tests.Unit.Bybit.ByBitUsdPerpetualOrderMonitorTests;

public class UnsubscribeFromOrderUpdatesTests : ByBitUsdPerpetualOrderMonitorTestsBase
{
    [Fact]
    public async Task UnsubscribeFromOrderUpdatesAsync_ShouldUnsubscribeFromOrderUpdates_WhenSubscribed()
    {
        // Arrange
        await this.SUT.SubscribeToOrderUpdatesAsync();

        // Act
        await this.SUT.UnsubscribeFromOrderUpdatesAsync();

        // Assert
        this.SUT.Subscribed.Should().BeFalse();
        await this.UsdPerpetualStreams.UnsubscribeAsync(Arg.Is<int>(id => id == this.UpdateSubscriptionCallResult.Data.Id));
    }

    [Fact]
    public async Task UnsubscribeFromKlineUpdatesAsync_ShouldThrow_WhenNotSubscribed()
    {
        // Act
        var func = async () => await this.SUT.UnsubscribeFromOrderUpdatesAsync();
        
        // Assert
        await func.Should().ThrowExactlyAsync<NotSubscribedException>().WithMessage("Not subscribed to perpetual order updates");
    }
}