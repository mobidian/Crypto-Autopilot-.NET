using Application.Data.Mapping;

using Infrastructure.Tests.Integration.DataAccess.FuturesOrdersRepositoryTests.AbstractBase;

namespace Infrastructure.Tests.Integration.DataAccess.FuturesOrdersRepositoryTests;

public class GetAllFuturesOrdersTests : FuturesOrdersRepositoryTestsBase
{
    [Test]
    public async Task GetAllFuturesOrders_ShouldReturnAllFuturesOrders_WhenFuturesOrdersExist()
    {
        // Arrange
        var futuresOrders = this.FuturesOrdersGenerator.GenerateBetween(1, 5);
        await this.ArrangeAssertDbContext.FuturesOrders.AddRangeAsync(futuresOrders.Select(x => x.ToDbEntity()));
        await this.ArrangeAssertDbContext.SaveChangesAsync();

        // Act
        var retrievedFuturesOrders = await this.SUT.GetAllFuturesOrdersAsync();

        // Assert
        futuresOrders.ForEach(x => retrievedFuturesOrders.Should().ContainEquivalentOf(x));
    }

    [Test]
    public async Task GetAllFuturesOrders_ShouldReturnEmptyEnumerable_WhenNoFuturesOrdersExist()
    {
        // Act
        var retrievedFuturesOrders = await this.SUT.GetAllFuturesOrdersAsync();

        // Assert
        retrievedFuturesOrders.Should().BeEmpty();
    }
}
