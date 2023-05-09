using Application.Data.Mapping;

using Infrastructure.Tests.Integration.DataAccess.FuturesOrdersRepositoryTests.AbstractBase;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Integration.DataAccess.FuturesOrdersRepositoryTests;

public class DeleteFuturesOrderTests : FuturesOrdersRepositoryTestsBase
{
    [Test]
    public async Task DeleteFuturesOrder_ShouldDeleteFuturesOrder_WhenFuturesOrderExists()
    {
        // Arrange
        var futuresOrder = this.FuturesOrdersGenerator.Generate();
        await this.ArrangeAssertDbContext.FuturesOrders.AddAsync(futuresOrder.ToDbEntity());
        await this.ArrangeAssertDbContext.SaveChangesAsync();

        // Act
        await this.SUT.DeleteFuturesOrdersAsync(futuresOrder.BybitID);

        // Assert
        this.ArrangeAssertDbContext.FuturesOrders.Should().BeEmpty();
    }

    [Test]
    public async Task DeleteFuturesOrder_ShouldThrow_WhenFuturesOrderDoesNotExist()
    {
        // Arrange
        var bybitId = Guid.NewGuid();

        // Act
        var func = async () => await this.SUT.DeleteFuturesOrdersAsync(bybitId);

        // Assert
        await func.Should().ThrowExactlyAsync<DbUpdateException>().WithMessage($"No order with bybitID {bybitId} was found in the database");
    }
}
