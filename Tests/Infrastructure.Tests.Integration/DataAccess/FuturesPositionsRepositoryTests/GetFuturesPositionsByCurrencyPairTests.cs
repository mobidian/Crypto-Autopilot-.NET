using Application.Data.Mapping;

using Infrastructure.Tests.Integration.DataAccess.FuturesPositionsRepositoryTests.AbstractBase;

namespace Infrastructure.Tests.Integration.DataAccess.FuturesPositionsRepositoryTests;

public class GetFuturesPositionsByCurrencyPairTests : FuturesPositionsRepositoryTestsBase
{
    [Test]
    public async Task GetFuturesPositionsByCurrencyPair_ShouldReturnAllFuturesPositionsWithCurrencyPair_WhenFuturesPositionsWithCurrencyPairExist()
    {
        // Arrange
        var currencyPair = this.CurrencyPairGenerator.Generate();
        var futuresPositions = this.FuturesPositionsGenerator.Clone().RuleFor(o => o.CurrencyPair, currencyPair).Generate(15);
        await this.ArrangeAssertDbContext.FuturesPositions.AddRangeAsync(futuresPositions.Select(x => x.ToDbEntity()));
        await this.ArrangeAssertDbContext.SaveChangesAsync();

        for (var i = 0; i < 5; i++)
        {
            var diffrentCurrencyPair = this.CurrencyPairGenerator.Generate();
            var futuresOrdersWithDiffrentCurrencyPair = this.FuturesPositionsGenerator.Clone().RuleFor(o => o.CurrencyPair, diffrentCurrencyPair).Generate(15);
            await this.ArrangeAssertDbContext.FuturesPositions.AddRangeAsync(futuresOrdersWithDiffrentCurrencyPair.Select(x => x.ToDbEntity()));
            await this.ArrangeAssertDbContext.SaveChangesAsync();
        }


        // Act
        var retrievedFuturesPositions = await this.SUT.GetFuturesPositionsByCurrencyPairAsync(currencyPair.Name);

        // Assert
        retrievedFuturesPositions.Should().BeEquivalentTo(futuresPositions);
    }

    [Test]
    public async Task GetFuturesPositionsByCurrencyPair_ShouldReturnEmptyEnumerable_WhenNoFuturesPositionsWithCurrencyPairExist()
    {
        // Arrange
        var currencyPair = this.CurrencyPairGenerator.Generate();

        for (var i = 0; i < 5; i++)
        {
            var diffrentCurrencyPair = this.CurrencyPairGenerator.Generate();
            var futuresPositionsWithDiffrentCurrencyPair = this.FuturesPositionsGenerator.Clone().RuleFor(o => o.CurrencyPair, diffrentCurrencyPair).Generate(15);
            await this.ArrangeAssertDbContext.FuturesPositions.AddRangeAsync(futuresPositionsWithDiffrentCurrencyPair.Select(x => x.ToDbEntity()));
            await this.ArrangeAssertDbContext.SaveChangesAsync();
        }


        // Act
        var retrievedFuturesPositions = await this.SUT.GetFuturesPositionsByCurrencyPairAsync(currencyPair.Name);

        // Assert
        retrievedFuturesPositions.Should().BeEmpty();
    }
}
