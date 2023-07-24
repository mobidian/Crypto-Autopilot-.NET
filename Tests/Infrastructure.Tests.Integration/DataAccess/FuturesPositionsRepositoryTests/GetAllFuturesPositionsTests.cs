using Application.Data.Mapping;

using Bogus;

using FluentAssertions;

using Infrastructure.Tests.Integration.DataAccess.FuturesPositionsRepositoryTests.AbstractBase;

using Tests.Integration.Common.Fixtures;

using Xunit;

namespace Infrastructure.Tests.Integration.DataAccess.FuturesPositionsRepositoryTests;

public class GetAllFuturesPositionsTests : FuturesPositionsRepositoryTestsBase
{
    public GetAllFuturesPositionsTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }


    [Fact]
    public async Task GetAllFuturesPositions_ShouldReturnAllFuturesPositions_WhenFuturesPositionsExist()
    {
        // Arrange
        var futuresPositions = this.FuturesPositionsGenerator.GenerateBetween(1, 5);
        await this.ArrangeAssertDbContext.FuturesPositions.AddRangeAsync(futuresPositions.Select(x => x.ToDbEntity()));
        await this.ArrangeAssertDbContext.SaveChangesAsync();

        // Act
        var retrievedFuturesPositions = await this.SUT.GetAllAsync();

        // Assert
        futuresPositions.ForEach(x => retrievedFuturesPositions.Should().ContainEquivalentOf(x));
    }

    [Fact]
    public async Task GetAllFuturesPositions_ShouldReturnEmptyEnumerable_WhenNoFuturesPositionsExist()
    {
        // Act
        var retrievedFuturesPositions = await this.SUT.GetAllAsync();

        // Assert
        retrievedFuturesPositions.Should().BeEmpty();
    }
}
