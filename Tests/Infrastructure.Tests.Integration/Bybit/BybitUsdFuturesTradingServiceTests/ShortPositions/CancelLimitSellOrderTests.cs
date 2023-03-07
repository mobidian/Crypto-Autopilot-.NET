using Application.Exceptions;

using Bybit.Net.Enums;

using Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.AbstractBase;

namespace Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.ShortPositions;

public class CancelLimitSellOrderTests : BybitUsdFuturesTradingServiceTestsBase
{
    [Test]
    public async Task CancelLimitOrder_ShouldCancelLimitSellOrder_WhenLimitSellOrderExists()
    {
        // Arrange
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var limitPrice = lastPrice + 500;
        var stopLoss = limitPrice + 300;
        var takeProfit = limitPrice - 300;
        var tradingStopTriggerType = TriggerType.LastPrice;
        await this.SUT.PlaceLimitOrderAsync(OrderSide.Sell, limitPrice, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);

        // Act
        await this.SUT.CancelLimitOrderAsync(OrderSide.Sell);

        // Assert
        this.SUT.BuyLimitOrder.Should().BeNull();
    }

    [Test]
    public async Task CancelLimitOrder_ShouldThrow_WhenLimitSellOrderDoesNotExist()
    {
        // Act
        var func = async () => await this.SUT.CancelLimitOrderAsync(OrderSide.Sell);

        // Assert
        await func.Should().ThrowAsync<InvalidOrderException>().WithMessage("No open Sell limit order was found");
    }
}
