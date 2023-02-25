using Application.Exceptions;

using Binance.Net.Enums;

using Infrastructure.Tests.Integration.BinanceCfdTradingServiceTests.Base;

namespace Infrastructure.Tests.Integration.BinanceCfdTradingServiceTests;

public class UpdatePositionTakeProfitTests : BinanceCfdTradingServiceTestsBase
{
    private const decimal precision = 1; // for assertions
    
    
    [Test, Order(1)]
    public async Task PlaceTakeProfitAsync_ShouldUpdateTakeProfit_WhenPositionExistsAndInputIsCorrect([Random(1.001, 1.01, 1, Distinct = true)] decimal prc)
    {
        // Arrange
        var current_price = await this.SUT.GetCurrentPriceAsync();
        var new_take_profit_price = prc * current_price;
        await this.SUT.PlaceMarketOrderAsync(OrderSide.Buy, this.testMargin, 0.99m * current_price, 1.01m * current_price);

        // Act
        var newTakeProfitPlacedOrder = await this.SUT.PlaceTakeProfitAsync(new_take_profit_price);


        // Assert
        var newTakeProfitOrder = await this.SUT.GetOrderAsync(this.SUT.Position!.TakeProfitOrder!.Id);

        this.SUT.Position!.TakeProfitPrice.Should().BeApproximately(new_take_profit_price, precision);
        newTakeProfitOrder.Id.Should().Be(newTakeProfitPlacedOrder.Id);
        newTakeProfitOrder.StopPrice.Should().Be(newTakeProfitPlacedOrder.StopPrice);
    }
    
    [Test, Order(2)]
    public async Task PlaceTakeProfitAsync_ShouldNotUpdateTakeProfit_WhenPositionExistsButInputIsIncorrect()
    {
        // Arrange
        var current_price = await this.SUT.GetCurrentPriceAsync();
        await this.SUT.PlaceMarketOrderAsync(OrderSide.Buy, this.testMargin, 0.99m * current_price, 1.01m * current_price);
        var initial_take_profit_price = this.SUT.Position!.TakeProfitPrice!.Value;

        // Act
        var func = async () => await this.SUT.PlaceTakeProfitAsync(-1);


        // Assert
        await func.Should().ThrowExactlyAsync<InternalTradingServiceException>().WithMessage("The take profit could not be placed | Error: -1102: Mandatory parameter 'stopPrice' was not sent, was empty/null, or malformed.");
        this.SUT.Position!.TakeProfitPrice.Should().Be(initial_take_profit_price);
        
        var takeProfitPlacedOrder = await this.SUT.GetOrderAsync(this.SUT.Position!.TakeProfitOrder!.Id);
        takeProfitPlacedOrder.StopPrice.Should().Be(initial_take_profit_price);
    }
    
    [Test, Order(3)]
    public async Task PlaceTakeProfitAsync_ShouldThrow_WhenPositionDoesNotExist()
    {
        // Act
        var func = async () => await this.SUT.PlaceTakeProfitAsync(-1);
        
        // Assert
        await func.Should().ThrowExactlyAsync<InvalidOperationException>().WithMessage("No position is open thus a take profit can't be placed");
        this.SUT.Position!.Should().BeNull();
    }
}