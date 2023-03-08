using Application.Data.Mapping;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests;

public class GetAllFuturesOrdersByCurrencyPairTests : FuturesTradesDBServiceTestsBase
{
    [Test]
    public async Task GetAllFuturesOrdersByCurrencyPair_ShouldReturnAllFuturesOrdersWithCurrencyPair_WhenFuturesOrdersWithCurrencyPairExist()
    {
        // Arrange
        var currencyPair = this.CurrencyPairGenerator.Generate();
        var futuresOrders = this.FuturesOrderGenerator.Clone().RuleFor(o => o.CurrencyPair, currencyPair).Generate(15);
        await this.DbContext.FuturesOrders.AddRangeAsync(futuresOrders.Select(x => x.ToDbEntity()).ToArray());
        await this.DbContext.SaveChangesAsync();
        
        for (var i = 0; i < 5; i++)
        {
            var diffrentCurrencyPair = this.CurrencyPairGenerator.Generate();
            var futuresOrdersWithDiffrentCurrencyPair = this.FuturesOrderGenerator.Clone().RuleFor(o => o.CurrencyPair, diffrentCurrencyPair).Generate(15);
            await this.DbContext.FuturesOrders.AddRangeAsync(futuresOrdersWithDiffrentCurrencyPair.Select(x => x.ToDbEntity()).ToArray());
            await this.DbContext.SaveChangesAsync();
        }


        // Act
        var retrievedFuturesOrders = await this.SUT.GetFuturesOrdersByCurrencyPairAsync(currencyPair.Name);

        // Assert
        retrievedFuturesOrders.Should().BeEquivalentTo(futuresOrders);
    }

    [Test]
    public async Task GetAllFuturesOrdersByCurrencyPair_ShouldReturnEmptyEnumerable_WhenNoFuturesOrdersWithCurrencyPairExist()
    {
        // Arrange
        var currencyPair = this.CurrencyPairGenerator.Generate();
        
        for (var i = 0; i < 5; i++)
        {
            var diffrentCurrencyPair = this.CurrencyPairGenerator.Generate();
            var futuresOrdersWithDiffrentCurrencyPair = this.FuturesOrderGenerator.Clone().RuleFor(o => o.CurrencyPair, diffrentCurrencyPair).Generate(15);
            await this.DbContext.FuturesOrders.AddRangeAsync(futuresOrdersWithDiffrentCurrencyPair.Select(x => x.ToDbEntity()).ToArray());
            await this.DbContext.SaveChangesAsync();
        }
        
        
        // Act
        var retrievedFuturesOrders = await this.SUT.GetFuturesOrdersByCurrencyPairAsync(currencyPair.Name);

        // Assert
        retrievedFuturesOrders.Should().BeEmpty();
    }
}
