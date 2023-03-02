using Application.Exceptions;

using Binance.Net.Enums;

namespace Infrastructure.Tests.Integration.BinanceFuturesTradingServiceTestsBase.LongPositions;

public class UpdateLongPositionStopLossTests : Base.BinanceFuturesTradingServiceTestsBase
{
    [Test]
    public async Task PlaceStopLossAsync_ShouldUpdateStopLoss_WhenPositionExistsAndInputIsCorrect([Random(0.99, 0.999, 1, Distinct = true)] decimal prc)
    {
        // Arrange
        var current_price = await this.MarketDataProvider.GetCurrentPriceAsync(this.CurrencyPair.Name);
        var new_stop_loss_price = prc * current_price;
        await this.SUT.PlaceMarketOrderAsync(OrderSide.Buy, this.Margin, 0.99m * current_price, 1.01m * current_price);
        
        // Act
        var newStopLossOrder = await this.SUT.PlaceStopLossAsync(new_stop_loss_price);

        // Assert
        this.SUT.Position!.StopLossPrice.Should().BeApproximately(new_stop_loss_price, precision);
        newStopLossOrder.Id.Should().Be(newStopLossOrder.Id);
        newStopLossOrder.StopPrice.Should().Be(newStopLossOrder.StopPrice);
    }

    [Test]
    public async Task PlaceStopLossAsync_ShouldThrow_WhenPositionExistsAndPriceIsGreaterThanCurrentPrice()
    {
        // Arrange
        var current_price = await this.MarketDataProvider.GetCurrentPriceAsync(this.CurrencyPair.Name);
        var new_stop_loss_price = current_price + 10;
        await this.SUT.PlaceMarketOrderAsync(OrderSide.Buy, this.Margin, 0.99m * current_price, 1.01m * current_price);
        
        // Act
        var func = async () => await this.SUT.PlaceStopLossAsync(new_stop_loss_price);
        
        // Assert
        await func.Should().ThrowExactlyAsync<InternalTradingServiceException>().WithMessage("-2021: Order would immediately trigger.");
    }
}
