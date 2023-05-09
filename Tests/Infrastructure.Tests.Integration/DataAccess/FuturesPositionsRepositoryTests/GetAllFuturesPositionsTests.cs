using Application.Data.Mapping;

using Infrastructure.Tests.Integration.DataAccess.FuturesPositionsRepositoryTests.AbstractBase;

namespace Infrastructure.Tests.Integration.DataAccess.FuturesPositionsRepositoryTests;

public class GetAllFuturesPositionsTests : FuturesPositionsRepositoryTestsBase
{
    [Test]
    public async Task GetAllFuturesPositions_ShouldReturnAllFuturesPositions_WhenFuturesPositionsExist()
    {
        // Arrange
        var futuresPositions = this.FuturesPositionsGenerator.GenerateBetween(1, 5);
        await this.ArrangeAssertDbContext.FuturesPositions.AddRangeAsync(futuresPositions.Select(x => x.ToDbEntity()));
        await this.ArrangeAssertDbContext.SaveChangesAsync();

        // Act
        var retrievedFuturesPositions = await this.SUT.GetAllFuturesPositionsAsync();

        // Assert
        futuresPositions.ForEach(x => retrievedFuturesPositions.Should().ContainEquivalentOf(x));
    }

    [Test]
    public async Task GetAllFuturesPositions_ShouldReturnEmptyEnumerable_WhenNoFuturesPositionsExist()
    {
        // Act
        var retrievedFuturesPositions = await this.SUT.GetAllFuturesPositionsAsync();

        // Assert
        retrievedFuturesPositions.Should().BeEmpty();
    }
}
