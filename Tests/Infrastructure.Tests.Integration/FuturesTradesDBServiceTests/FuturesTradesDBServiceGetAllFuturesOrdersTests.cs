using Application.Mapping;

using Binance.Net.Objects.Models.Futures;

using Domain.Models;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests;

public class GetAllFuturesOrdersTests : FuturesTradesDBServiceTestsBase
{
    [Test]
    public async Task GetAllFuturesOrders_ShouldReturnAllFuturesOrders()
    {
        // Arrange
        var orders = new List<BinanceFuturesOrder>();
        for (int i = 0; i < 15; i++)
            orders.AddRange(await this.InsertOneCandleAndMultipleFuturesOrdersAsync());
        
        // Act
        var retrievedFuturesOrders = await this.SUT.GetAllFuturesOrdersAsync();

        // Assert
        orders.ForEach(x => retrievedFuturesOrders.Should().ContainEquivalentOf(x));
    }
    private async Task<List<BinanceFuturesOrder>> InsertOneCandleAndMultipleFuturesOrdersAsync()
    {
        var candlestick = this.CandlestickGenerator.Generate();
        var futuresOrders = this.FuturesOrderGenerator.GenerateBetween(1, 5);
        futuresOrders.ForEach(order => order.Symbol = candlestick.CurrencyPair.Name);

        using (var transaction = await this.dbContext.Database.BeginTransactionAsync())
        {
            var candlesticksEntity = candlestick.ToDbEntity();
            var futuresOrdersEntities = futuresOrders.Select(x => x.ToDbEntity()).ToList();

            await this.dbContext.Candlesticks.AddAsync(candlesticksEntity);
            await this.dbContext.SaveChangesAsync();

            futuresOrdersEntities.ForEach(x => x.CandlestickId = candlesticksEntity.Id);

            this.dbContext.FuturesOrders.AddRange(futuresOrdersEntities);
            await this.dbContext.SaveChangesAsync();

            await transaction.CommitAsync();
        }

        return futuresOrders;
    }
}
