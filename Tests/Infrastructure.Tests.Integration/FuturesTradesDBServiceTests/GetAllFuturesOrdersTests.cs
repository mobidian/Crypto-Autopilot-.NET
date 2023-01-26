using Binance.Net.Objects.Models.Futures;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests;

public class GetAllFuturesOrdersTests : FuturesTradesDBServiceTestsBase
{
    [Test]
    public async Task GetAllFuturesOrders_ShouldReturnAllFuturesOrders_WhenFuturesOrdersExist()
    {
        // Arrange
        var orders = new List<BinanceFuturesOrder>();
        for (int i = 0; i < 10; i++)
        {
            var futuresOrders = this.FuturesOrderGenerator.GenerateBetween(1, 5);
            await this.InsertOneCandlestickAndMultipleFuturesOrdersAsync(this.CandlestickGenerator.Generate(), futuresOrders);

            orders.AddRange(futuresOrders);
        }
        
        // Act
        var retrievedFuturesOrders = await this.SUT.GetAllFuturesOrdersAsync();

        // Assert
        orders.ForEach(x => retrievedFuturesOrders.Should().ContainEquivalentOf(x));
    }

    [Test]
    public async Task GetAllFuturesOrders_ShouldReturnEmptyEnumerable_WhenNoFuturesOrdersExist()
    {
        // Act
        var retrievedFuturesOrders = await this.SUT.GetAllFuturesOrdersAsync();

        // Assert
        retrievedFuturesOrders.Should().BeEmpty();
    }
}
