using Application.Exceptions;

using Binance.Net.Enums;

using Infrastructure.Services.Trading.Internal.Enums;

namespace Infrastructure.Tests.Integration.BinanceFuturesTradingServiceTestsBase.LongPositions;

public class UpdateLongPositionTakeProfitTests : Base.BinanceFuturesTradingServiceTestsBase
{
    [Test]
    public async Task PlaceTakeProfitAsync_ShouldUpdateTakeProfit_WhenPositionExistsAndInputIsCorrect([Random(1.001, 1.01, 1, Distinct = true)] decimal prc)
    {
        // Arrange
        var current_price = await this.MarketDataProvider.GetCurrentPriceAsync(this.CurrencyPair.Name);
        var new_take_profit_price = prc * current_price;
        await this.SUT.PlaceMarketOrderAsync(OrderSide.Buy, this.Margin, 0.99m * current_price, 1.01m * current_price);

        // Act
        var newTakeProfitOrder = await this.SUT.PlaceTakeProfitAsync(new_take_profit_price);

        // Assert
        this.SUT.Position!.TakeProfitPrice.Should().BeApproximately(new_take_profit_price, precision);
        this.SUT.Position!.TakeProfitOrder!.Id.Should().Be(newTakeProfitOrder.Id);
        this.SUT.Position!.TakeProfitOrder!.StopPrice.Should().Be(newTakeProfitOrder.StopPrice);

        this.SUT.OcoTaskStatus.Should().Be(OrderMonitoringTaskStatus.Running);

        this.SUT.OcoIDs!.StopLoss.Should().Be(this.SUT.Position.StopLossOrder!.Id);
        this.SUT.OcoIDs!.TakeProfit.Should().Be(this.SUT.Position.TakeProfitOrder!.Id);
    }

    [Test]
    public async Task PlaceTakeProfitAsync_ShouldThrow_WhenPositionExistsAndPriceIsLessGreaterThanCurrentPrice()
    {
        // Arrange
        var current_price = await this.MarketDataProvider.GetCurrentPriceAsync(this.CurrencyPair.Name);
        var new_take_profit_price = current_price - 10;
        await this.SUT.PlaceMarketOrderAsync(OrderSide.Buy, this.Margin, 0.99m * current_price, 1.01m * current_price);
        
        // Act
        var func = async () => await this.SUT.PlaceTakeProfitAsync(new_take_profit_price);
        
        // Assert
        await func.Should().ThrowExactlyAsync<InternalTradingServiceException>().WithMessage("-2021: Order would immediately trigger.");
    }
}
