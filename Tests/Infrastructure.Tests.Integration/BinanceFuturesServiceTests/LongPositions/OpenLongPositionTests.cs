using Application.Exceptions;

using Binance.Net.Enums;

using Infrastructure.Tests.Integration.BinanceFuturesApiServiceTestsBase.Base;

namespace Infrastructure.Tests.Integration.BinanceFuturesApiServiceTestsBase.LongPositions;

public class OpenLongPositionTests : BinanceFuturesApiServiceTestsBaseClass
{
    [Test]
    public async Task OpenPosition_ShouldOpenLongPosition_WhenInputIsCorrect()
    {
        // Arrange
        var current_price = await this.MarketDataProvider.GetCurrentPriceAsync(this.CurrencyPair.Name);
        
        // Act
        var orders = await this.SUT_PlaceMarketOrderAsync(this.CurrencyPair.Name, OrderSide.Buy, this.Margin, this.Leverage, 0.99m * current_price, 1.01m * current_price);
        
        // Assert
        var ordersArray = orders.ToArray();
        var entryOrder = ordersArray[0];
        var stopLossOrder = ordersArray[1];
        var takeProfitOrder = ordersArray[2];

        entryOrder.Side.Should().Be(OrderSide.Buy);
        entryOrder.PositionSide.Should().Be(PositionSide.Long);
        entryOrder.Quantity.Should().BeApproximately(this.Margin * this.Leverage / current_price, precision);
        
        stopLossOrder.StopPrice.Should().BeApproximately(0.99m * current_price, precision);
        takeProfitOrder.StopPrice.Should().BeApproximately(1.01m * current_price, precision);
    }
    
    [Test]
    public async Task OpenPosition_ShouldNotOpenLongPosition_WhenInputIsIncorrect()
    {
        // Arrange
        var current_price = await this.MarketDataProvider.GetCurrentPriceAsync(this.CurrencyPair.Name);
        
        // Act
        var func = async () => await this.SUT_PlaceMarketOrderAsync(this.CurrencyPair.Name, OrderSide.Buy, this.Margin, this.Leverage, 1.01m * current_price, 0.99m * current_price);

        // Assert
        await func.Should().ThrowExactlyAsync<InvalidOrderException>();
    }
}
