using Application.Exceptions;

using Binance.Net.Enums;

using Infrastructure.Tests.Integration.BinanceFuturesTradingServiceTestsBase.Base;

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
        var newStopLossPlacedOrder = await this.SUT.PlaceStopLossAsync(new_stop_loss_price);


        // Assert
        var newStopLossOrder = await this.MarketDataProvider.GetOrderAsync(this.CurrencyPair.Name, this.SUT.Position!.StopLossOrder!.Id);

        this.SUT.Position!.StopLossPrice.Should().BeApproximately(new_stop_loss_price, precision);
        newStopLossOrder.Id.Should().Be(newStopLossPlacedOrder.Id);
        newStopLossOrder.StopPrice.Should().Be(newStopLossPlacedOrder.StopPrice);
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
        await func.Should().ThrowExactlyAsync<InternalTradingServiceException>().WithMessage("The stop loss could not be placed | Error: -2021: Order would immediately trigger.");
    }
}
