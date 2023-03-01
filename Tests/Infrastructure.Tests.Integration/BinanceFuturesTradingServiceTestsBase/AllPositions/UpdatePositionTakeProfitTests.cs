using Application.Exceptions;

using Binance.Net.Enums;

using Infrastructure.Tests.Integration.BinanceCfdTradingServiceTests.Base;

namespace Infrastructure.Tests.Integration.BinanceCfdTradingServiceTests.AllPositions;

public class UpdatePositionTakeProfitTests : BinanceFuturesTradingServiceTestsBase
{
    [Test]
    public async Task PlaceTakeProfitAsync_ShouldNotUpdateTakeProfit_WhenPositionExistsButPriceIsInvalid()
    {
        // Arrange
        var current_price = await this.MarketDataProvider.GetCurrentPriceAsync(this.CurrencyPair.Name);
        await (Random.Shared.Next(2) switch
        {
            0 => this.SUT.PlaceMarketOrderAsync(OrderSide.Buy, this.testMargin, 0.99m * current_price, 1.01m * current_price),
            1 => this.SUT.PlaceMarketOrderAsync(OrderSide.Sell, this.testMargin, 1.01m * current_price, 0.99m * current_price),
            _ => throw new NotImplementedException()
        });
        var initial_take_profit_price = this.SUT.Position!.TakeProfitPrice!.Value;

        // Act
        var func = async () => await this.SUT.PlaceTakeProfitAsync(-1);


        // Assert
        await func.Should().ThrowExactlyAsync<InternalTradingServiceException>().WithMessage("The take profit could not be placed | Error: -1102: Mandatory parameter 'stopPrice' was not sent, was empty/null, or malformed.");
        this.SUT.Position!.TakeProfitPrice.Should().Be(initial_take_profit_price);

        var takeProfitPlacedOrder = await this.MarketDataProvider.GetOrderAsync(this.CurrencyPair.Name, this.SUT.Position!.TakeProfitOrder!.Id);
        takeProfitPlacedOrder.StopPrice.Should().Be(initial_take_profit_price);
    }
    
    [Test]
    public async Task PlaceTakeProfitAsync_ShouldThrow_WhenPositionDoesNotExist()
    {
        // Act
        var func = async () => await this.SUT.PlaceTakeProfitAsync(-1);

        // Assert
        await func.Should().ThrowExactlyAsync<InvalidOperationException>().WithMessage("No position is open thus a take profit can't be placed");
        this.SUT.Position!.Should().BeNull();
    }
}