using Binance.Net.Enums;

using Infrastructure.Tests.Unit.OrderStatusMonitorTests.Base;

namespace Infrastructure.Tests.Unit.OrderStatusMonitorTests;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[Parallelizable(ParallelScope.All)]
public class WaitForAnyOrderToReachStatusTests : OrderStatusMonitorTestsBase
{
    [Test]
    public async Task WaitForAnyOrderToReachStatusAsync_ShouldWaitForAnyOrderStatusToReachSpecifiedStatus_WhenSubscribed()
    {
        // Arrange
        var orderIDs = Enumerable.Range(0, 10).Select(_ => Random.Shared.NextInt64()).ToList();
        var finalStatus = OrderStatus.Filled;

        await this.SUT.SubscribeToOrderUpdatesAsync();
        await Task.Delay(100);

        
        // Act
        var task = this.SUT.WaitForAnyOrderToReachStatusAsync(orderIDs, finalStatus);
        await Task.Delay(100);
        var taskCompletedBeforeFinalStatus = task.IsCompleted;

        for (var i = 0; i < 3; i++)
        {
            this.SUT.HandleOrderUpdate(this.CreateDataEvent(orderIDs[Random.Shared.Next(orderIDs.Count)], OrderStatus.PartiallyFilled)); // the dictionary value will get updated here
            await Task.Delay(100);
            taskCompletedBeforeFinalStatus = task.IsCompleted;
        }
        
        this.SUT.HandleOrderUpdate(this.CreateDataEvent(orderIDs[Random.Shared.Next(orderIDs.Count)], finalStatus)); // the dictionary value will get updated here
        await Task.Delay(100);
        var taskCompletedAfterFinalStatus = task.IsCompleted;

        
        // Assert
        taskCompletedBeforeFinalStatus.Should().BeFalse();
        taskCompletedAfterFinalStatus.Should().BeTrue();
    }
}
