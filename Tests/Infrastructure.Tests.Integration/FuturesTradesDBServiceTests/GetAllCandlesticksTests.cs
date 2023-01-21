using Application.Mapping;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests;

public class GetAllCandlesticksTests : FuturesTradesDBServiceTestsBase
{
    [Test]
    public async Task GetAllCandlesticks_ShouldReturnAllCandlesticks()
    {
        // Arrange
        var candlesticks = this.CandlestickGenerator.GenerateBetween(100, 300);

        using (var transaction = await this.dbContext.Database.BeginTransactionAsync())
        {
            this.dbContext.Candlesticks.AddRange(candlesticks.Select(x => x.ToDbEntity()));
            await this.dbContext.SaveChangesAsync();

            await transaction.CommitAsync();
        }


        // Act
        var retrievedCandlesticks = await this.SUT.GetAllCandlesticksAsync();
        
        // Assert
        retrievedCandlesticks.Should().BeEquivalentTo(candlesticks);
    }
}
