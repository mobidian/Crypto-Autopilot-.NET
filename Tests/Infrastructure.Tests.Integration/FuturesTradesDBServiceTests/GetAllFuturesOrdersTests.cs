using Application.Data.Mapping;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests;

public class GetAllFuturesOrdersTests : FuturesTradesDBServiceTestsBase
{
    [Test]
    public async Task GetAllFuturesOrders_ShouldReturnAllFuturesOrders_WhenFuturesOrdersExist()
    {
        // Arrange
        var futuresOrders = this.FuturesOrderGenerator.GenerateBetween(1, 5);
        await this.DbContext.FuturesOrders.AddRangeAsync(futuresOrders.Select(x => x.ToDbEntity()).ToArray());
        await this.DbContext.SaveChangesAsync();

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
