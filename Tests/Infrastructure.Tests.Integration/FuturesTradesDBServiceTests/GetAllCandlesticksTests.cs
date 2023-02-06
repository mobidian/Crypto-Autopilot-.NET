using Application.Mapping;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests;

public class GetAllCandlesticksTests : FuturesTradesDBServiceTestsBase
{
    [Test]
    public async Task GetAllCandlesticks_ShouldReturnAllCandlesticks_WhenCandlesticksExist()
    {
        // Arrange
        var candlesticks = this.CandlestickGenerator.GenerateBetween(100, 300);

        using (var transaction = await this.DbContext.Database.BeginTransactionAsync())
        {
            this.DbContext.Candlesticks.AddRange(candlesticks.Select(x => x.ToDbEntity()));
            await this.DbContext.SaveChangesAsync();

            await transaction.CommitAsync();
        }


        // Act
        var retrievedCandlesticks = await this.SUT.GetAllCandlesticksAsync();
        
        // Assert
        retrievedCandlesticks.Should().BeEquivalentTo(candlesticks);
    }

    [Test]
    public async Task GetAllCandlesticks_ShouldReturnEmptyEnumerable_WhenNoCandlesticksExist()
    {
        // Act
        var retrievedCandlesticks = await this.SUT.GetAllCandlesticksAsync();

        // Assert
        retrievedCandlesticks.Should().BeEmpty();
    }
}
