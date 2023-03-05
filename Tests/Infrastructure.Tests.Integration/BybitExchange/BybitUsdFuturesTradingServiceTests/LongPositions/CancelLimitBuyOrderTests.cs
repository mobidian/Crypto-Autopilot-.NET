using Application.Exceptions;

using Bybit.Net.Enums;

using Infrastructure.Tests.Integration.BybitExchange.BybitUsdFuturesTradingServiceTests.AbstractBase;

namespace Infrastructure.Tests.Integration.BybitExchange.BybitUsdFuturesTradingServiceTests.LongPositions;

public class CancelLimitBuyOrderTests : BybitUsdFuturesTradingServiceTestsBase
{
    [Test]
    public async Task CancelLimitOrder_ShouldCancelLimitBuyOrder_WhenLimitBuyOrderExists()
    {
        // Arrange
        var lastPrice = await this.FuturesDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var limitPrice = lastPrice - 500;
        var stopLoss = limitPrice - 300;
        var takeProfit = limitPrice + 300;
        var tradingStopTriggerType = TriggerType.LastPrice;
        await this.SUT.PlaceLimitOrderAsync(OrderSide.Buy, limitPrice, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);

        // Act
        await this.SUT.CancelLimitOrderAsync(OrderSide.Buy);
        
        // Assert
        this.SUT.BuyLimitOrder.Should().BeNull();
    }

    [Test]
    public async Task CancelLimitOrder_ShouldThrow_WhenLimitBuyOrderDoesNotExist()
    {
        // Act
        var func = async () => await this.SUT.CancelLimitOrderAsync(OrderSide.Buy);
        
        // Assert
        await func.Should().ThrowAsync<InvalidOrderException>().WithMessage("No open Buy limit order was found");
    }
}
