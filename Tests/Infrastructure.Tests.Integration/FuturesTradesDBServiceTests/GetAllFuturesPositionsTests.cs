using Application.Data.Mapping;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests;

public class GetAllFuturesPositionsTests : FuturesTradesDBServiceTestsBase
{
    [Test]
    public async Task GetAllFuturesOrders_ShouldReturnAllFuturesOrders_WhenFuturesOrdersExist()
    {
        // Arrange
        var futuresPositions = this.FuturesPositionsGenerator.GenerateBetween(1, 5);
        await this.DbContext.FuturesPositions.AddRangeAsync(futuresPositions.Select(x => x.ToDbEntity()));
        await this.DbContext.SaveChangesAsync();

        // Act
        var retrievedFuturesPositions = await this.SUT.GetAllFuturesPositionsAsync();

        // Assert
        futuresPositions.ForEach(x => retrievedFuturesPositions.Should().ContainEquivalentOf(x));
    }

    [Test]
    public async Task GetAllFuturesOrders_ShouldReturnEmptyEnumerable_WhenNoFuturesOrdersExist()
    {
        // Act
        var retrievedFuturesPositions = await this.SUT.GetAllFuturesPositionsAsync();

        // Assert
        retrievedFuturesPositions.Should().BeEmpty();
    }
}
