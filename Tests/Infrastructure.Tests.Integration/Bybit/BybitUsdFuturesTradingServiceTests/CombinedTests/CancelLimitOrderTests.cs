using Bybit.Net.Enums;

using Domain.Models;

using Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.AbstractBase;

namespace Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.CombinedTests;

public class CancelLimitOrderTests : BybitUsdFuturesTradingServiceTestsBase
{
    [TestCase(OrderSide.Buy, -500)]
    [TestCase(OrderSide.Sell, 500)]
    public async Task CancelLimitOrder_ShouldCancelLimitOrder_WhenLimitOrderExists(OrderSide orderSide, decimal limitPriceOffset)
    {
        // Arrange
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var limitPrice = lastPrice + limitPriceOffset;

        var order = await this.SUT.PlaceLimitOrderAsync(orderSide, limitPrice, this.Margin);

        
        // Act
        await this.SUT.CancelLimitOrdersAsync(order.BybitID);

        
        // Assert
        this.SUT.LimitOrders.Should().BeEmpty();
    }

    [Test]
    public async Task CancelLimitOrders_ShouldCancelOnlySpecifiedOrders_WhenMultipleLimitOrdersExist()
    {
        // Arrange
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        
        var orders = new List<FuturesOrder>();
        for (var offset = 100; offset <= 500; offset += 100)
            orders.Add(Random.Shared.Next(2) switch
            {
                0 => await this.SUT.PlaceLimitOrderAsync(OrderSide.Buy, lastPrice - offset, this.Margin),
                1 => await this.SUT.PlaceLimitOrderAsync(OrderSide.Sell, lastPrice + offset, this.Margin),
                _ => throw new NotImplementedException(),
            });

        
        // Act
        var ids = orders.Take(3).Select(x => x.BybitID).ToArray();
        await this.SUT.CancelLimitOrdersAsync(ids);

        
        // Assert
        this.SUT.LimitOrders.Should().BeEquivalentTo(orders.Skip(3));
    }


    [Test]
    public async Task CancelAllLimitOrdersAsync_ShouldCancelAllLimitOrders()
    {
        // Arrange
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);

        await this.SUT.PlaceLimitOrderAsync(OrderSide.Buy, lastPrice - 500, this.Margin);
        await this.SUT.PlaceLimitOrderAsync(OrderSide.Sell, lastPrice + 500, this.Margin);

        // Act
        await this.SUT.CancelAllLimitOrdersAsync();

        // Assert
        this.SUT.LimitOrders.Should().BeEmpty();
    }
}
