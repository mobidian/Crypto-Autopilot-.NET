using Binance.Net.Enums;

using Infrastructure.Services.Trading.Internal.Enums;

namespace Infrastructure.Tests.Integration.BinanceFuturesTradingServiceTestsBase.ShortPositions;

public class CloseShortPositionTests : Base.BinanceFuturesTradingServiceTestsBase
{
    [Test]
    public async Task ClosePosition_ShouldCloseShortPosition_WhenShortPositionExists()
    {
        // Arrange
        var current_price = await this.MarketDataProvider.GetCurrentPriceAsync(this.CurrencyPair.Name);
        var orders = await this.SUT.PlaceMarketOrderAsync(OrderSide.Sell, this.Margin, 1.01m * current_price, 0.99m * current_price);

        var ordersArray = orders.ToArray();
        var entryOrder = ordersArray[0];
        var stopLoss = ordersArray[1];
        var takeProfit = ordersArray[2];


        // Act
        await this.SUT.ClosePositionAsync();


        // Assert
        this.SUT.IsInPosition().Should().BeFalse();

        var shortPosition = await this.AccountDataProvider.GetPositionAsync(this.CurrencyPair.Name, PositionSide.Short);
        entryOrder = await this.MarketDataProvider.GetOrderAsync(entryOrder.Symbol, entryOrder.Id);
        stopLoss = await this.MarketDataProvider.GetOrderAsync(stopLoss.Symbol, stopLoss.Id);
        takeProfit = await this.MarketDataProvider.GetOrderAsync(takeProfit.Symbol, takeProfit.Id);

        shortPosition.Should().BeNull();
        entryOrder.Status.Should().Be(OrderStatus.Filled);
        stopLoss.Status.Should().Be(OrderStatus.Canceled);
        takeProfit.Status.Should().Be(OrderStatus.Canceled);

        this.SUT.OcoTaskStatus.Should().Be(OcoTaskStatus.Cancelled);
        
        this.SUT.OcoIDs.Should().BeNull();
    }
}
