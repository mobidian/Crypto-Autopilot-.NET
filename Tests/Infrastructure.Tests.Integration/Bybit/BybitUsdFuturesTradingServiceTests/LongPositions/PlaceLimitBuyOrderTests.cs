using Application.Exceptions;

using Bybit.Net.Enums;

using Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.AbstractBase;

namespace Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.LongPositions;

public class PlaceLimitBuyOrderTests : BybitUsdFuturesTradingServiceTestsBase
{
    [Test]
    public async Task PlaceLimitOrder_ShouldPlaceBuyLimitOrder_WhenNoBuyLimitOrderExists()
    {
        // Arrange
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var limitPrice = lastPrice - 500;
        var stopLoss = limitPrice - 300;
        var takeProfit = limitPrice + 300;
        var tradingStopTriggerType = TriggerType.LastPrice;

        // Act
        await this.SUT.PlaceLimitOrderAsync(OrderSide.Buy, limitPrice, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);

        // Assert
        this.SUT.BuyLimitOrder.Should().NotBeNull();
        this.SUT.BuyLimitOrder!.Side.Should().Be(OrderSide.Buy);
        this.SUT.BuyLimitOrder!.Price.Should().Be(limitPrice);
        this.SUT.BuyLimitOrder!.Quantity.Should().Be(Math.Round(this.Margin * this.Leverage / limitPrice, 2));
        this.SUT.BuyLimitOrder!.StopLoss.Should().Be(stopLoss);
        this.SUT.BuyLimitOrder!.StopLossTriggerType.Should().Be(tradingStopTriggerType);
        this.SUT.BuyLimitOrder!.TakeProfit.Should().Be(takeProfit);
        this.SUT.BuyLimitOrder!.TakeProfitTriggerType.Should().Be(tradingStopTriggerType);
    }

    [Test]
    public async Task PlaceLimitOrder_ShouldThrow_WhenLimitPriceIsIncorrect()
    {
        // Arrange
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var limitPrice = lastPrice + 500;
        var stopLoss = limitPrice - 300;
        var takeProfit = limitPrice + 300;
        var tradingStopTriggerType = TriggerType.LastPrice;

        // Act
        var func = async () => await this.SUT.PlaceLimitOrderAsync(OrderSide.Buy, limitPrice, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);

        // Assert
        await func.Should().ThrowExactlyAsync<InternalTradingServiceException>();
    }

    [Test]
    public async Task PlaceLimitOrder_ShouldThrow_WhenTradingStopParametersAreIncorrect()
    {
        // Arrange
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var limitPrice = lastPrice - 500;
        var stopLoss = limitPrice + 300;
        var takeProfit = limitPrice - 300;
        var tradingStopTriggerType = TriggerType.LastPrice;

        // Act
        var func = async () => await this.SUT.PlaceLimitOrderAsync(OrderSide.Buy, limitPrice, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);

        // Assert
        await func.Should().ThrowExactlyAsync<InternalTradingServiceException>();
    }

    [Test]
    public async Task PlaceLimitOrder_ShouldThrow_WhenLimitBuyOrderAlreadyExists()
    {
        // Arrange
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var limitPrice = lastPrice - 500;
        var stopLoss = limitPrice - 300;
        var takeProfit = limitPrice + 300;
        var tradingStopTriggerType = TriggerType.LastPrice;
        await this.SUT.PlaceLimitOrderAsync(OrderSide.Buy, limitPrice, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);

        // Act
        var func = async () => await this.SUT.PlaceLimitOrderAsync(OrderSide.Buy, limitPrice, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);

        // Assert
        await func.Should().ThrowExactlyAsync<InvalidOrderException>("There is an open Buy limit order already");
    }
}
