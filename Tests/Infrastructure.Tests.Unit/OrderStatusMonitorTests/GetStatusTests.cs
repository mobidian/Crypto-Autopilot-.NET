using Binance.Net.Enums;

using Infrastructure.Tests.Unit.OrderStatusMonitorTests.Base;

namespace Infrastructure.Tests.Unit.OrderStatusMonitorTests;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[Parallelizable(ParallelScope.All)]
public class GetStatusTests : OrderStatusMonitorTestsBase
{
    [TearDown]
    public void TearDown()
    {
        this.OrdersStatuses.Clear();
    }


    [Test]
    public async Task GetStatusAsync_ShouldRetrunOrderStatus_WhenOrderIsInDictionaryAndIsNotNull()
    {
        // Arrange
        var OrderId = Random.Shared.Next();
        var Status = (OrderStatus)Random.Shared.Next(Enum.GetValues<OrderStatus>().Length);
        this.OrdersStatuses[OrderId] = Status;

        // Act
        var returnedStatus = await this.SUT.GetStatusAsync(OrderId);

        // Assert
        returnedStatus.Should().Be(Status);
    }


    [Test]
    public async Task GetStatusAsync_ShouldWaitForOrderStatusToNotBeNull_WhenOrderIsInDictionaryAndItIsNull()
    {
        // Arrange
        var OrderId = Random.Shared.Next();
        this.OrdersStatuses[OrderId] = null;
        var firstDataEvent = this.CreateDataEvent(OrderId, (OrderStatus) Random.Shared.Next(Enum.GetValues<OrderStatus>().Length));
        
        // Act
        var task = this.SUT.GetStatusAsync(OrderId);
        await Task.Delay(100);
        var taskCompletedBeforeStatusHasValue = task.IsCompleted;
        
        this.SUT.HandleOrderUpdate(firstDataEvent); // the dictionary value will get updated here
        await Task.Delay(100);
        var taskCompletedAfterStatusHasValue = task.IsCompleted;


        // Assert
        taskCompletedBeforeStatusHasValue.Should().BeFalse();
        taskCompletedAfterStatusHasValue.Should().BeTrue();
    }


    [Test]
    public void GetStatusAsync_ShouldThrow_WhenKeyDoesNotExist()
    {
        // Arrange
        var randomId = Random.Shared.Next();

        // Act
        var func = async () => await this.SUT.GetStatusAsync(randomId);
        
        // Assert
        func.Should().ThrowExactlyAsync<KeyNotFoundException>().WithMessage($"The given key '{randomId}' was not present in the dictionary.");
    }
}
