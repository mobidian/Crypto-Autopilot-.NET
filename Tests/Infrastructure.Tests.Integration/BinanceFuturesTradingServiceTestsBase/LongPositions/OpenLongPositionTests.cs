using Application.Exceptions;

using Binance.Net.Enums;

namespace Infrastructure.Tests.Integration.BinanceFuturesTradingServiceTestsBase.LongPositions;

public class OpenLongPositionTests : Base.BinanceFuturesTradingServiceTestsBase
{
    [Test]
    public async Task OpenPosition_ShouldOpenLongPosition_WhenInputIsCorrect()
    {
        // Arrange
        var current_price = await this.MarketDataProvider.GetCurrentPriceAsync(this.CurrencyPair.Name);

        // Act
        await this.SUT.PlaceMarketOrderAsync(OrderSide.Buy, this.testMargin, 0.99m * current_price, 1.01m * current_price);

        // Assert
        this.SUT.IsInPosition().Should().BeTrue();
        this.SUT.Position!.CurrencyPair.Should().Be(this.CurrencyPair);
        this.SUT.Position!.Side.Should().Be(PositionSide.Long);
        this.SUT.Position!.Margin.Should().Be(this.testMargin);
        this.SUT.Position!.StopLossOrder.Should().NotBeNull();
        this.SUT.Position!.TakeProfitOrder.Should().NotBeNull();

        this.SUT.Position.StopLossPrice.Should().BeApproximately(0.99m * this.SUT.Position.EntryPrice, precision);
        this.SUT.Position.TakeProfitPrice.Should().BeApproximately(1.01m * this.SUT.Position.EntryPrice, precision);
        
        var longPosition = await this.AccountDataProvider.GetPositionAsync(this.CurrencyPair.Name, PositionSide.Long);
        longPosition.Symbol.Should().Be(this.CurrencyPair.Name);
        longPosition.Should().NotBeNull();
        longPosition.PositionSide.Should().Be(PositionSide.Long);
        longPosition.EntryPrice.Should().BeApproximately(current_price, precision);
        longPosition.Quantity.Should().BeApproximately(this.testMargin * this.Leverage / current_price, precision);
    }

    [Test]
    public async Task OpenPosition_ShouldNotOpenLongPosition_WhenInputIsIncorrect()
    {
        // Arrange
        var current_price = await this.MarketDataProvider.GetCurrentPriceAsync(this.CurrencyPair.Name);


        // Act
        var func = async () => await this.SUT.PlaceMarketOrderAsync(OrderSide.Buy, this.testMargin, 1.01m * current_price, 0.99m * current_price);


        // Assert
        await func.Should().ThrowExactlyAsync<InvalidOrderException>();

        var longPosition = await this.AccountDataProvider.GetPositionAsync(this.CurrencyPair.Name, PositionSide.Long);
        longPosition.Should().BeNull();
    }
}
