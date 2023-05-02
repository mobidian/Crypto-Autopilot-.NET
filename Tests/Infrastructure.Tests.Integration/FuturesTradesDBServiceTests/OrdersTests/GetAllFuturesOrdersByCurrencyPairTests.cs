using Application.Data.Mapping;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.OrdersTests;

public class GetFuturesOrdersByCurrencyPairTests : FuturesTradesDBServiceTestsBase
{
    [Test]
    public async Task GetFuturesOrdersByCurrencyPair_ShouldReturnAllFuturesOrdersWithCurrencyPair_WhenFuturesOrdersWithCurrencyPairExist()
    {
        // Arrange
        var currencyPair = this.CurrencyPairGenerator.Generate();
        var futuresOrders = this.FuturesOrdersGenerator.Clone().RuleFor(o => o.CurrencyPair, currencyPair).Generate(15);
        await this.DbContext.FuturesOrders.AddRangeAsync(futuresOrders.Select(x => x.ToDbEntity()).ToArray());
        await this.DbContext.SaveChangesAsync();

        for (var i = 0; i < 5; i++)
        {
            var diffrentCurrencyPair = this.CurrencyPairGenerator.Generate();
            var futuresOrdersWithDiffrentCurrencyPair = this.FuturesOrdersGenerator.Clone().RuleFor(o => o.CurrencyPair, diffrentCurrencyPair).Generate(15);
            await this.DbContext.FuturesOrders.AddRangeAsync(futuresOrdersWithDiffrentCurrencyPair.Select(x => x.ToDbEntity()).ToArray());
            await this.DbContext.SaveChangesAsync();
        }


        // Act
        var retrievedFuturesOrders = await this.SUT.GetFuturesOrdersByCurrencyPairAsync(currencyPair.Name);

        // Assert
        retrievedFuturesOrders.Should().BeEquivalentTo(futuresOrders);
    }
    
    [Test]
    public async Task GetFuturesOrdersByCurrencyPair_ShouldReturnEmptyEnumerable_WhenNoFuturesOrdersWithCurrencyPairExist()
    {
        // Arrange
        var currencyPair = this.CurrencyPairGenerator.Generate();

        for (var i = 0; i < 5; i++)
        {
            var diffrentCurrencyPair = this.CurrencyPairGenerator.Generate();
            var futuresOrdersWithDiffrentCurrencyPair = this.FuturesOrdersGenerator.Clone().RuleFor(o => o.CurrencyPair, diffrentCurrencyPair).Generate(15);
            await this.DbContext.FuturesOrders.AddRangeAsync(futuresOrdersWithDiffrentCurrencyPair.Select(x => x.ToDbEntity()).ToArray());
            await this.DbContext.SaveChangesAsync();
        }


        // Act
        var retrievedFuturesOrders = await this.SUT.GetFuturesOrdersByCurrencyPairAsync(currencyPair.Name);

        // Assert
        retrievedFuturesOrders.Should().BeEmpty();
    }
}
