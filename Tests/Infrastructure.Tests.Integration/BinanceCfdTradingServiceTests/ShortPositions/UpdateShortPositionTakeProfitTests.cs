using Application.Exceptions;

using Binance.Net.Enums;

using Infrastructure.Tests.Integration.BinanceCfdTradingServiceTests.Base;

namespace Infrastructure.Tests.Integration.BinanceCfdTradingServiceTests.ShortPositions;

public class UpdateShortPositionTakeProfitTests : BinanceCfdTradingServiceTestsBase
{
    [Test]
    public async Task PlaceTakeProfitAsync_ShouldUpdateTakeProfit_WhenPositionExistsAndInputIsCorrect([Random(0.99, 0.999, 1, Distinct = true)] decimal prc)
    {
        // Arrange
        var current_price = await this.MarketDataProvider.GetCurrentPriceAsync(this.CurrencyPair.Name);
        var new_take_profit_price = prc * current_price;
        await this.SUT.PlaceMarketOrderAsync(OrderSide.Sell, this.testMargin, 1.01m * current_price, 0.99m * current_price);

        // Act
        var newTakeProfitPlacedOrder = await this.SUT.PlaceTakeProfitAsync(new_take_profit_price);


        // Assert
        var newTakeProfitOrder = await this.SUT.GetOrderAsync(this.SUT.Position!.TakeProfitOrder!.Id);

        this.SUT.Position!.TakeProfitPrice.Should().BeApproximately(new_take_profit_price, precision);
        newTakeProfitOrder.Id.Should().Be(newTakeProfitPlacedOrder.Id);
        newTakeProfitOrder.StopPrice.Should().Be(newTakeProfitPlacedOrder.StopPrice);
    }
    
    [Test]
    public async Task PlaceTakeProfitAsync_ShouldThrow_WhenPositionExistsAndPriceIsGreaterThanCurrentPrice()
    {
        // Arrange
        var current_price = await this.MarketDataProvider.GetCurrentPriceAsync(this.CurrencyPair.Name);
        var new_take_profit_price = current_price + 10;
        await this.SUT.PlaceMarketOrderAsync(OrderSide.Sell, this.testMargin, 1.01m * current_price, 0.99m * current_price);
        
        // Act
        var func = async () => await this.SUT.PlaceTakeProfitAsync(new_take_profit_price);

        // Assert
        await func.Should().ThrowExactlyAsync<InternalTradingServiceException>().WithMessage("The take profit could not be placed | Error: -2021: Order would immediately trigger.");
    }
}
