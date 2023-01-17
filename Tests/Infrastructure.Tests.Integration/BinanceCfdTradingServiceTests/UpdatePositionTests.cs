using Application.Exceptions;

using Binance.Net.Enums;

using Infrastructure.Tests.Integration.BinanceCfdTradingServiceTests.Common;

namespace Infrastructure.Tests.Integration.BinanceCfdTradingServiceTests;

public class UpdatePositionTests : BinanceCfdTradingServiceTestsBase
{
    private const decimal precision = 1; // for assertions


    [Test, Order(1)]
    public async Task UpdateStopLoss_ShouldUpdateStopLoss_WhenPositionExistsAndInputIsCorrect([Random(0.99, 0.999, 1, Distinct = true)] decimal prc)
    {
        // Arrange
        decimal current_price = await this.SUT.GetCurrentPriceAsync();
        await this.SUT.OpenPositionAtMarketPriceAsync(OrderSide.Buy, this.testMargin, 0.99m * current_price, 1.01m * current_price);

        // Act
        var placedOrder = await this.SUT.PlaceStopLossAsync(prc * current_price);


        // Assert
        var futuresOrder = await this.SUT.GetOrderAsync(this.SUT.Position!.StopLossOrder!.Id);
        
        this.SUT.Position!.StopLossPrice.Should().BeApproximately(prc * current_price, precision);
        futuresOrder.Id.Should().Be(placedOrder.Id);
        futuresOrder.StopPrice.Should().Be(placedOrder.StopPrice);
    }
    
    [Test, Order(2)]
    public async Task UpdateStopLoss_ShouldNotUpdateStopLoss_WhenPositionExistsButInputIsIncorrect([Random(1.001, 1.01, 1, Distinct = true)] decimal prc)
    {
        // Arrange
        decimal current_price = await this.SUT.GetCurrentPriceAsync();
        await this.SUT.OpenPositionAtMarketPriceAsync(OrderSide.Buy, this.testMargin, 0.99m * current_price, 1.01m * current_price);
        decimal initial_stop_loss = this.SUT.Position!.StopLossPrice!.Value;

        // Act
        var func = async () => await this.SUT.PlaceStopLossAsync(-1);
        

        // Assert
        await func.Should().ThrowExactlyAsync<InternalTradingServiceException>().WithMessage("The stop loss could not be placed | Error: -1102: Mandatory parameter 'stopPrice' was not sent, was empty/null, or malformed.");
        this.SUT.Position!.StopLossPrice.Should().Be(initial_stop_loss);

        var futuresOrder = await this.SUT.GetOrderAsync(this.SUT.Position!.StopLossOrder!.Id);
        futuresOrder.StopPrice.Should().Be(initial_stop_loss);
    }
    
    [Test, Order(3)]
    public async Task UpdateStopLoss_ShouldThrow_WhenPositionDoesNotExist()
    {
        // Act
        var func = async () => await this.SUT.PlaceStopLossAsync(-1);
        
        // Assert
        await func.Should().ThrowExactlyAsync<InvalidOperationException>().WithMessage("No position is open thus a stop loss can't be placed");
        this.SUT.Position!.Should().BeNull();
    }
}
