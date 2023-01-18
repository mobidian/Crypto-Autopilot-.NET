using Application.Mapping;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests;

public class FuturesTradesDBServiceGetAllFuturesOrdersTests : FuturesTradesDBServiceTestsBase
{
    [Test]
    public async Task GetAllFuturesOrders_ShouldReturnAllFuturesOrders()
    {
        // Arrange
        var candlesticks = this.CandlestickGenerator.GenerateBetween(100, 300);
        var futuresOrders = candlesticks.Select(_ => this.FuturesOrderGenerator.Generate()).ToList();

        using (var transaction = await this.dbContext.Database.BeginTransactionAsync())
        {
            var candlesticksEntities = candlesticks.Select(x => x.ToDbEntity()).ToList();
            var futuresOrdersEntities = futuresOrders.Select(x => x.ToDbEntity()).ToList();

            this.dbContext.Candlesticks.AddRange(candlesticksEntities);
            await this.dbContext.SaveChangesAsync();
             
            for (int i = 0; i < candlesticksEntities.Count; i++)
                futuresOrdersEntities[i].CandlestickId = candlesticksEntities[i].Id;

            this.dbContext.FuturesOrders.AddRange(futuresOrdersEntities);
            await this.dbContext.SaveChangesAsync();


            await transaction.CommitAsync();
        }


        // Act
        var retrievedFuturesOrders = await this.SUT.GetAllFuturesOrdersAsync();

        // Assert
        retrievedFuturesOrders.Should().BeEquivalentTo(futuresOrders);
    }
}
