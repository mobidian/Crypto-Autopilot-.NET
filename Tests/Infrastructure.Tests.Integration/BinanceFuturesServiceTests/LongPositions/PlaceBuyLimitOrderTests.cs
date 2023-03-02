using System.Text;

using Application.Exceptions;

using Binance.Net.Enums;

using Infrastructure.Tests.Integration.BinanceFuturesApiServiceTestsBase.Base;

namespace Infrastructure.Tests.Integration.BinanceFuturesApiServiceTestsBase.LongPositions;

public class PlaceBuyLimitOrderTests : BinanceFuturesApiServiceTestsBaseClass
{
    [Test]
    public async Task PlaceLimitOrderAsync_ShouldPlaceBuyLimitOrder_WhenLimitPriceAndTpSlAreCorrect()
    {
        // Arrange
        var current_price = await this.MarketDataProvider.GetCurrentPriceAsync(this.CurrencyPair.Name);
        var limitPrice = current_price - 50;
        var stopLoss = limitPrice - 25;
        var takeProfit = limitPrice + 25;

        // Act
        var order = await this.SUT_PlaceLimitOrderAsync(this.CurrencyPair.Name, OrderSide.Buy, limitPrice, this.Margin, this.Leverage, stopLoss, takeProfit);

        // Assert
        order.Price.Should().Be(limitPrice);
    }

    [Test]
    public async Task PlaceLimitOrderAsync_ShouldThrow_WhenLimitPriceIsIncorrect()
    {
        // Arrange
        var current_price = await this.MarketDataProvider.GetCurrentPriceAsync(this.CurrencyPair.Name);
        var limitPrice = current_price + 50;
        var stopLoss = limitPrice - 25;
        var takeProfit = limitPrice + 25;

        // Act
        var func = async () => await this.SUT_PlaceLimitOrderAsync(this.CurrencyPair.Name, OrderSide.Buy, limitPrice, this.Margin, this.Leverage, stopLoss, takeProfit);

        // Assert
        await func.Should().ThrowExactlyAsync<InvalidOrderException>().WithMessage("The limit price for a buy order can't be greater than the current price");
    }
    
    [Test]
    public async Task PlaceLimitOrderAsync_ShouldThrow_WhenTpSlAreIncorrect()
    {
        // Arrange
        var current_price = await this.MarketDataProvider.GetCurrentPriceAsync(this.CurrencyPair.Name);
        var limitPrice = current_price - 50;
        var stopLoss = limitPrice + 25; // greater than limit price
        var takeProfit = limitPrice - 25; // less greater than limit price

        // Act
        var func = async () => await this.SUT_PlaceLimitOrderAsync(this.CurrencyPair.Name, OrderSide.Buy, limitPrice, this.Margin, this.Leverage, stopLoss, takeProfit);
        
        // Assert
        var exceptionMessageBuilder = new StringBuilder();
        exceptionMessageBuilder.AppendLine($"The stop loss can't be greater than or equal to the limit price for a buy order, limit price was {limitPrice} and stop loss was {stopLoss}");
        exceptionMessageBuilder.Append($"The take profit can't be less greater than or equal to the limit price for a buy order, limit price was {limitPrice} and take profit was {takeProfit}");
        
        await func.Should().ThrowExactlyAsync<InvalidOrderException>().WithMessage(exceptionMessageBuilder.ToString());
    }
}
