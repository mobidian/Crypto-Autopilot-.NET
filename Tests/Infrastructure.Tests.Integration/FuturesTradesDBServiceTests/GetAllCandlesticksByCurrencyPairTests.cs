using Application.Mapping;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests;

public class GetAllCandlesticksByCurrencyPairTests : FuturesTradesDBServiceTestsBase
{
    [Test]
    public async Task GetAllCandlesticksByCurrencyPair_ShouldReturnAllCandlesticksWithSpecifiedCurrencyPair_WhenCandlesticksWithSpecifiedCurrencyPairExist()
    {
        // Arrange
        var currencyPair = this.CurrencyPairGenerator.Generate();
        var candlesticks = this.CandlestickGenerator.Clone().RuleFor(c => c.CurrencyPair, currencyPair).Generate(100);
        var randomCandlesticks = this.CandlestickGenerator.Generate(300);
        
        using (var transaction = await this.dbContext.Database.BeginTransactionAsync())
        {
            this.dbContext.Candlesticks.AddRange(candlesticks.Select(x => x.ToDbEntity()));
            this.dbContext.Candlesticks.AddRange(randomCandlesticks.Select(x => x.ToDbEntity()));
            await this.dbContext.SaveChangesAsync();

            await transaction.CommitAsync();
        }


        // Act
        var retrievedCandlesticks = await this.SUT.GetCandlesticksByCurrencyPairAsync(currencyPair.Name);

        // Assert
        retrievedCandlesticks.Should().BeEquivalentTo(candlesticks);
    }

    [Test]
    public async Task GetAllCandlesticksByCurrencyPair_ShouldReturnEmptyEnumerable_WhenNoCandlesticksWithSpecifiedCurrencyPairExist()
    {
        // Arrange
        var currencyPair = this.CurrencyPairGenerator.Generate();
        var randomCandlesticks = this.CandlestickGenerator.Clone().RuleFor(c => c.CurrencyPair, f => GetRandomCurrencyPairExcept(f, currencyPair)).Generate(300);

        using (var transaction = await this.dbContext.Database.BeginTransactionAsync())
        {
            this.dbContext.Candlesticks.AddRange(randomCandlesticks.Select(x => x.ToDbEntity()));
            await this.dbContext.SaveChangesAsync();

            await transaction.CommitAsync();
        }


        // Act
        var retrievedCandlesticks = await this.SUT.GetCandlesticksByCurrencyPairAsync(currencyPair.Name);

        // Assert
        retrievedCandlesticks.Should().BeEmpty();
    }
}