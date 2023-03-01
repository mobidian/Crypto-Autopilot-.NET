using Application.Exceptions;

using Binance.Net.Enums;

using Infrastructure.Tests.Integration.BinanceFuturesTradingServiceTestsBase.Base;

namespace Infrastructure.Tests.Integration.BinanceFuturesTradingServiceTestsBase.ShortPositions;

public class OpenShortPositionTests : Base.BinanceFuturesTradingServiceTestsBase
{
    [Test]
    public async Task OpenPosition_ShouldOpenShortPosition_WhenInputIsCorrect()
    {
        // Arrange
        var current_price = await this.MarketDataProvider.GetCurrentPriceAsync(this.CurrencyPair.Name);

        // Act
        await this.SUT.PlaceMarketOrderAsync(OrderSide.Sell, this.testMargin, 1.01m * current_price, 0.99m * current_price);

        // Assert
        this.SUT.IsInPosition().Should().BeTrue();
        this.SUT.Position!.Side.Should().Be(PositionSide.Short);
        this.SUT.Position!.Margin.Should().Be(this.testMargin);
        this.SUT.Position!.StopLossOrder.Should().NotBeNull();
        this.SUT.Position!.TakeProfitOrder.Should().NotBeNull();

        this.SUT.Position.StopLossPrice.Should().BeApproximately(1.01m * this.SUT.Position.EntryPrice, precision);
        this.SUT.Position.TakeProfitPrice.Should().BeApproximately(0.99m * this.SUT.Position.EntryPrice, precision);
    }

    [Test]
    public async Task OpenPosition_ShouldNotOpenShortPosition_WhenInputIsIncorrect()
    {
        // Arrange
        var current_price = await this.MarketDataProvider.GetCurrentPriceAsync(this.CurrencyPair.Name);

        // Act
        var func = async () => await this.SUT.PlaceMarketOrderAsync(OrderSide.Sell, this.testMargin, 0.99m * current_price, 1.01m * current_price);

        // Assert
        await func.Should().ThrowExactlyAsync<InvalidOrderException>();
    }
}
