using Binance.Net.Enums;

using Infrastructure.Services.Trading.Binance.Internal.Enums;

namespace Infrastructure.Tests.Integration.Binance.BinanceFuturesTradingServiceTestsBase.LongPositions;

public class CloseLongPositionTests : Base.BinanceFuturesTradingServiceTestsBase
{
    [Test]
    public async Task ClosePosition_ShouldCloseLongPosition_WhenLongPositionExists()
    {
        // Arrange
        var current_price = await this.MarketDataProvider.GetCurrentPriceAsync(this.CurrencyPair.Name);
        var orders = await this.SUT.PlaceMarketOrderAsync(OrderSide.Buy, this.Margin, 0.99m * current_price, 1.01m * current_price);

        var ordersArray = orders.ToArray();
        var entryOrder = ordersArray[0];
        var stopLoss = ordersArray[1];
        var takeProfit = ordersArray[2];


        // Act
        await this.SUT.ClosePositionAsync();


        // Assert
        this.SUT.IsInPosition().Should().BeFalse();

        var longPosition = await this.AccountDataProvider.GetPositionAsync(this.CurrencyPair.Name, PositionSide.Long);
        entryOrder = await this.MarketDataProvider.GetOrderAsync(entryOrder.Symbol, entryOrder.Id);
        stopLoss = await this.MarketDataProvider.GetOrderAsync(stopLoss.Symbol, stopLoss.Id);
        takeProfit = await this.MarketDataProvider.GetOrderAsync(takeProfit.Symbol, takeProfit.Id);

        longPosition.Should().BeNull();
        entryOrder.Status.Should().Be(OrderStatus.Filled);
        stopLoss.Status.Should().Be(OrderStatus.Canceled);
        takeProfit.Status.Should().Be(OrderStatus.Canceled);

        this.SUT.OcoTaskStatus.Should().Be(OrderMonitoringTaskStatus.Cancelled);

        this.SUT.OcoIDs.Should().BeNull();
    }
}
