using Application.Exceptions;

using Bybit.Net.Enums;

using FluentAssertions;

using Infrastructure.Tests.Unit.Bybit.ByBitUsdPerpetualOrderMonitorTests.AbstractBase;

using Xunit;

namespace Infrastructure.Tests.Unit.Bybit.ByBitUsdPerpetualOrderMonitorTests;

public class WaitForOrderToReachStatusTests : ByBitUsdPerpetualOrderMonitorTestsBase
{
    [Fact]
    public async Task WaitForOrderToReachStatusAsync_ShouldWaitForOrderStatusToReachSpecifiedStatus_WhenSubscribed()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var finalStatus = OrderStatus.Filled;

        await this.SUT.SubscribeToOrderUpdatesAsync();
        

        // Act
        var task = this.SUT.WaitForOrderToReachStatusAsync(orderId, finalStatus);
        await Task.Delay(100);
        var taskCompletedBeforeFinalStatus = task.IsCompleted;
        
        for (var i = 0; i < 3; i++)
        {
            this.SUT.HandleUsdPerpetualOrderUpdate(this.CreateDataEvent(orderId, OrderStatus.PartiallyFilled)); // the dictionary value will get updated here
            await Task.Delay(100);
            taskCompletedBeforeFinalStatus = task.IsCompleted;
        }
        
        this.SUT.HandleUsdPerpetualOrderUpdate(this.CreateDataEvent(orderId, finalStatus)); // the dictionary value will get updated here
        await Task.Delay(100);
        var taskCompletedAfterFinalStatus = task.IsCompleted;


        // Assert
        taskCompletedBeforeFinalStatus.Should().BeFalse();
        taskCompletedAfterFinalStatus.Should().BeTrue();
    }

    [Fact]
    public async Task WaitForOrderToReachStatusAsync_ShouldThrow_WhenNotSubscribed()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var finalStatus = OrderStatus.Filled;
        
        // Act
        var func = async () => await this.SUT.WaitForOrderToReachStatusAsync(orderId, finalStatus);
        
        // Assert
        await func.Should().ThrowAsync<NotSubscribedException>().WithMessage("Not subscribed to perpetual order updates");
    }
}
