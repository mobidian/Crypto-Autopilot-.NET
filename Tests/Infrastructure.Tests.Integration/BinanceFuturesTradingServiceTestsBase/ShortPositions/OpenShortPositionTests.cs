using Application.Exceptions;

using Binance.Net.Enums;

namespace Infrastructure.Tests.Integration.BinanceFuturesTradingServiceTestsBase.ShortPositions;

public class OpenShortPositionTests : Base.BinanceFuturesTradingServiceTestsBase
{
    [Test]
    public async Task OpenPosition_ShouldOpenShortPosition_WhenInputIsCorrect()
    {
        // Arrange
        var current_price = await this.MarketDataProvider.GetCurrentPriceAsync(this.CurrencyPair.Name);

        // Act
        await this.SUT.PlaceMarketOrderAsync(OrderSide.Sell, this.Margin, 1.01m * current_price, 0.99m * current_price);

        // Assert
        this.SUT.IsInPosition().Should().BeTrue();
        this.SUT.Position!.CurrencyPair.Should().Be(this.CurrencyPair);
        this.SUT.Position!.Side.Should().Be(PositionSide.Short);
        this.SUT.Position!.Margin.Should().Be(this.Margin);
        this.SUT.Position!.StopLossOrder.Should().NotBeNull();
        this.SUT.Position!.TakeProfitOrder.Should().NotBeNull();

        this.SUT.Position.StopLossPrice.Should().BeApproximately(1.01m * this.SUT.Position.EntryPrice, precision);
        this.SUT.Position.TakeProfitPrice.Should().BeApproximately(0.99m * this.SUT.Position.EntryPrice, precision);
        
        var shortPosition = await this.AccountDataProvider.GetPositionAsync(this.CurrencyPair.Name, PositionSide.Short);
        shortPosition.Symbol.Should().Be(this.CurrencyPair.Name);
        shortPosition.Should().NotBeNull();
        shortPosition.PositionSide.Should().Be(PositionSide.Short);
        shortPosition.EntryPrice.Should().BeApproximately(current_price, precision);
        shortPosition.Quantity.Should().BeApproximately(this.Margin * this.Leverage / current_price * -1, precision);
    }

    [Test]
    public async Task OpenPosition_ShouldNotOpenShortPosition_WhenInputIsIncorrect()
    {
        // Arrange
        var current_price = await this.MarketDataProvider.GetCurrentPriceAsync(this.CurrencyPair.Name);


        // Act
        var func = async () => await this.SUT.PlaceMarketOrderAsync(OrderSide.Sell, this.Margin, 0.99m * current_price, 1.01m * current_price);


        // Assert
        await func.Should().ThrowExactlyAsync<InvalidOrderException>();

        var shortPosition = await this.AccountDataProvider.GetPositionAsync(this.CurrencyPair.Name, PositionSide.Short);
        shortPosition.Should().BeNull();
    }
}
