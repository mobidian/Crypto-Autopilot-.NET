using Application.Data.Mapping;

using FluentAssertions;

using Infrastructure.Tests.Integration.DataAccess.FuturesOrdersRepositoryTests.AbstractBase;

using Tests.Integration.Common.Fixtures;

using Xunit;

namespace Infrastructure.Tests.Integration.DataAccess.FuturesOrdersRepositoryTests;

public class GetFuturesOrdersByCurrencyPairTests : FuturesOrdersRepositoryTestsBase
{
    public GetFuturesOrdersByCurrencyPairTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public async Task GetFuturesOrdersByCurrencyPair_ShouldReturnAllFuturesOrdersWithCurrencyPair_WhenFuturesOrdersWithCurrencyPairExist()
    {
        // Arrange
        var currencyPair = this.CurrencyPairGenerator.Generate();
        var futuresOrders = this.FuturesOrdersGenerator.Clone().RuleFor(o => o.CurrencyPair, currencyPair).Generate(15);
        await this.ArrangeAssertDbContext.FuturesOrders.AddRangeAsync(futuresOrders.Select(x => x.ToDbEntity()).ToArray());
        await this.ArrangeAssertDbContext.SaveChangesAsync();

        for (var i = 0; i < 5; i++)
        {
            var diffrentCurrencyPair = this.CurrencyPairGenerator.Generate();
            var futuresOrdersWithDiffrentCurrencyPair = this.FuturesOrdersGenerator.Clone().RuleFor(o => o.CurrencyPair, diffrentCurrencyPair).Generate(15);
            await this.ArrangeAssertDbContext.FuturesOrders.AddRangeAsync(futuresOrdersWithDiffrentCurrencyPair.Select(x => x.ToDbEntity()).ToArray());
            await this.ArrangeAssertDbContext.SaveChangesAsync();
        }


        // Act
        var retrievedFuturesOrders = await this.SUT.GetByCurrencyPairAsync(currencyPair.Name);

        // Assert
        retrievedFuturesOrders.Should().BeEquivalentTo(futuresOrders);
    }

    [Fact]
    public async Task GetFuturesOrdersByCurrencyPair_ShouldReturnEmptyEnumerable_WhenNoFuturesOrdersWithCurrencyPairExist()
    {
        // Arrange
        var currencyPair = this.CurrencyPairGenerator.Generate();

        for (var i = 0; i < 5; i++)
        {
            var diffrentCurrencyPair = this.CurrencyPairGenerator.Generate();
            var futuresOrdersWithDiffrentCurrencyPair = this.FuturesOrdersGenerator.Clone().RuleFor(o => o.CurrencyPair, diffrentCurrencyPair).Generate(15);
            await this.ArrangeAssertDbContext.FuturesOrders.AddRangeAsync(futuresOrdersWithDiffrentCurrencyPair.Select(x => x.ToDbEntity()).ToArray());
            await this.ArrangeAssertDbContext.SaveChangesAsync();
        }


        // Act
        var retrievedFuturesOrders = await this.SUT.GetByCurrencyPairAsync(currencyPair.Name);

        // Assert
        retrievedFuturesOrders.Should().BeEmpty();
    }
}
