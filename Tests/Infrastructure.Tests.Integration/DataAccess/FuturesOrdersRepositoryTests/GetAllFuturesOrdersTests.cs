using Application.Data.Mapping;

using Bogus;

using FluentAssertions;

using Infrastructure.Tests.Integration.Common.Fixtures;
using Infrastructure.Tests.Integration.DataAccess.FuturesOrdersRepositoryTests.AbstractBase;

using Xunit;

namespace Infrastructure.Tests.Integration.DataAccess.FuturesOrdersRepositoryTests;

public class GetAllFuturesOrdersTests : FuturesOrdersRepositoryTestsBase
{
    public GetAllFuturesOrdersTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }


    [Fact]
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

    [Fact]
    public async Task GetAllFuturesOrders_ShouldReturnEmptyEnumerable_WhenNoFuturesOrdersExist()
    {
        // Act
        var retrievedFuturesOrders = await this.SUT.GetAllFuturesOrdersAsync();

        // Assert
        retrievedFuturesOrders.Should().BeEmpty();
    }
}
