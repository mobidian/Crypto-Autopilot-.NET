using Application.Data.Mapping;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests;

public class DeleteFuturesOrderTests : FuturesTradesDBServiceTestsBase
{
    [Test]
    public async Task DeleteFuturesOrder_ShouldDeleteFuturesOrder_WhenFuturesOrderExists()
    {
        // Arrange
        var futuresOrder = this.FuturesOrderGenerator.Generate();
        await this.DbContext.FuturesOrders.AddAsync(futuresOrder.ToDbEntity());
        await this.DbContext.SaveChangesAsync();

        // Act
        await this.SUT.DeleteFuturesOrderAsync(futuresOrder.BybitID);
        
        // Assert
        this.DbContext.FuturesOrders.Should().BeEmpty();
    }

    [Test]
    public async Task DeleteFuturesOrder_ShouldThrow_WhenFuturesOrderDoesNotExist()
    {
        // Arrange
        var bybitId = Guid.NewGuid();

        // Act
        var func = async () => await this.SUT.DeleteFuturesOrderAsync(bybitId);
        
        // Assert
        await func.Should().ThrowExactlyAsync<DbUpdateException>().WithMessage($"No order with bybitID {bybitId} was found in the database");
    }
}
