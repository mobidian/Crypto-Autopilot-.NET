using Application.Exceptions;

using Bybit.Net.Enums;

using Infrastructure.Tests.Unit.Bybit.ByBitUsdPerpetualOrderMonitorTests.AbstractBase;

namespace Infrastructure.Tests.Unit.Bybit.ByBitUsdPerpetualOrderMonitorTests;

public class WaitForAnyOrderToReachStatusTests : ByBitUsdPerpetualOrderMonitorTestsBase
{
    [Test]
    public async Task WaitForAnyOrderToReachStatusAsync_ShouldWaitForAnyOrderStatusToReachSpecifiedStatus_WhenSubscribed()
    {
        // Arrange
        var orderIDs = Enumerable.Range(0, 10).Select(_ => Guid.NewGuid()).ToList();
        var finalStatus = OrderStatus.Filled;

        await this.SUT.SubscribeToOrderUpdatesAsync();
        

        // Act
        var task = this.SUT.WaitForAnyOrderToReachStatusAsync(orderIDs, finalStatus);
        await Task.Delay(100);
        var taskCompletedBeforeFinalStatus = task.IsCompleted;

        for (var i = 0; i < 3; i++)
        {
            this.SUT.HandleUsdPerpetualOrderUpdate(this.CreateDataEvent(orderIDs[Random.Shared.Next(orderIDs.Count)], OrderStatus.PartiallyFilled)); // the dictionary value will get updated here
            await Task.Delay(100);
            taskCompletedBeforeFinalStatus = task.IsCompleted;
        }
        
        this.SUT.HandleUsdPerpetualOrderUpdate(this.CreateDataEvent(orderIDs[Random.Shared.Next(orderIDs.Count)], finalStatus)); // the dictionary value will get updated here
        await Task.Delay(100);
        var taskCompletedAfterFinalStatus = task.IsCompleted;


        // Assert
        taskCompletedBeforeFinalStatus.Should().BeFalse();
        taskCompletedAfterFinalStatus.Should().BeTrue();
    }

    [Test]
    public async Task WaitForAnyOrderToReachStatusAsync_ShouldThrow_WhenNotSubscribed()
    {
        // Arrange
        var orderIDs = Enumerable.Range(0, 10).Select(_ => Guid.NewGuid()).ToList();
        var finalStatus = OrderStatus.Filled;

        // Act
        var func = async () => await this.SUT.WaitForAnyOrderToReachStatusAsync(orderIDs, finalStatus);

        // Assert
        await func.Should().ThrowAsync<NotSubscribedException>().WithMessage("Not subscribed to perpetual order updates");
    }
}