using Binance.Net.Enums;

using Infrastructure.Tests.Unit.Binance.OrderStatusMonitorTests.Base;

namespace Infrastructure.Tests.Unit.Binance.OrderStatusMonitorTests;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[Parallelizable(ParallelScope.All)]
public class WaitForOrderToReachStatusTests : OrderStatusMonitorTestsBase
{
    [Test]
    public async Task WaitForOrderToReachStatusAsync_ShouldWaitForOrderStatusToReachSpecifiedStatus_WhenSubscribed()
    {
        // Arrange
        var orderId = Random.Shared.Next();
        var finalStatus = OrderStatus.Filled;

        await this.SUT.SubscribeToOrderUpdatesAsync();
        await Task.Delay(100);


        // Act
        var task = this.SUT.WaitForOrderToReachStatusAsync(orderId, finalStatus);
        await Task.Delay(100);
        var taskCompletedBeforeFinalStatus = task.IsCompleted;

        for (var i = 0; i < 3; i++)
        {
            this.SUT.HandleOrderUpdate(this.CreateDataEvent(orderId, OrderStatus.PartiallyFilled)); // the dictionary value will get updated here
            await Task.Delay(100);
            taskCompletedBeforeFinalStatus = task.IsCompleted;
        }

        this.SUT.HandleOrderUpdate(this.CreateDataEvent(orderId, finalStatus)); // the dictionary value will get updated here
        await Task.Delay(100);
        var taskCompletedAfterFinalStatus = task.IsCompleted;


        // Assert
        taskCompletedBeforeFinalStatus.Should().BeFalse();
        taskCompletedAfterFinalStatus.Should().BeTrue();
    }
}
