using Application.Data.Mapping;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.PositionsTests;

public class GetFuturesPositionsByCurrencyPairTests : FuturesTradesDBServiceTestsBase
{
    [Test]
    public async Task GetFuturesPositionsByCurrencyPair_ShouldReturnAllFuturesPositionsWithCurrencyPair_WhenFuturesPositionsWithCurrencyPairExist()
    {
        // Arrange
        var currencyPair = this.CurrencyPairGenerator.Generate();
        var futuresPositions = this.FuturesPositionsGenerator.Clone().RuleFor(o => o.CurrencyPair, currencyPair).Generate(15);
        await this.DbContext.FuturesPositions.AddRangeAsync(futuresPositions.Select(x => x.ToDbEntity()));
        await this.DbContext.SaveChangesAsync();

        for (var i = 0; i < 5; i++)
        {
            var diffrentCurrencyPair = this.CurrencyPairGenerator.Generate();
            var futuresOrdersWithDiffrentCurrencyPair = this.FuturesPositionsGenerator.Clone().RuleFor(o => o.CurrencyPair, diffrentCurrencyPair).Generate(15);
            await this.DbContext.FuturesPositions.AddRangeAsync(futuresOrdersWithDiffrentCurrencyPair.Select(x => x.ToDbEntity()));
            await this.DbContext.SaveChangesAsync();
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
            await this.DbContext.FuturesPositions.AddRangeAsync(futuresPositionsWithDiffrentCurrencyPair.Select(x => x.ToDbEntity()));
            await this.DbContext.SaveChangesAsync();
        }


        // Act
        var retrievedFuturesPositions = await this.SUT.GetFuturesPositionsByCurrencyPairAsync(currencyPair.Name);

        // Assert
        retrievedFuturesPositions.Should().BeEmpty();
    }
}
