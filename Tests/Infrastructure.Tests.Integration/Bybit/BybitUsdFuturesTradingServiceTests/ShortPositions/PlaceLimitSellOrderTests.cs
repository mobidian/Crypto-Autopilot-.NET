using Application.Exceptions;

using Bybit.Net.Enums;

using Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.AbstractBase;

namespace Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.ShortPositions;

public class PlaceLimitSellOrderTests : BybitUsdFuturesTradingServiceTestsBase
{
    [Test]
    public async Task PlaceLimitOrder_ShouldPlaceSellLimitOrder_WhenNoSellLimitOrderExists()
    {
        // Arrange
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var limitPrice = lastPrice + 500;
        var stopLoss = limitPrice + 300;
        var takeProfit = limitPrice - 300;
        var tradingStopTriggerType = TriggerType.LastPrice;

        // Act
        await this.SUT.PlaceLimitOrderAsync(OrderSide.Sell, limitPrice, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);

        // Assert
        this.SUT.SellLimitOrder.Should().NotBeNull();
        this.SUT.SellLimitOrder!.Side.Should().Be(OrderSide.Sell);
        this.SUT.SellLimitOrder!.Price.Should().Be(limitPrice);
        this.SUT.SellLimitOrder!.Quantity.Should().Be(Math.Round(this.Margin * this.Leverage / limitPrice, 2));
        this.SUT.SellLimitOrder!.StopLoss.Should().Be(stopLoss);
        this.SUT.SellLimitOrder!.StopLossTriggerType.Should().Be(tradingStopTriggerType);
        this.SUT.SellLimitOrder!.TakeProfit.Should().Be(takeProfit);
        this.SUT.SellLimitOrder!.TakeProfitTriggerType.Should().Be(tradingStopTriggerType);
    }

    [Test]
    public async Task PlaceLimitOrder_ShouldThrow_WhenLimitPriceIsIncorrect()
    {
        // Arrange
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var limitPrice = lastPrice - 500;
        var stopLoss = limitPrice + 300;
        var takeProfit = limitPrice - 300;
        var tradingStopTriggerType = TriggerType.LastPrice;

        // Act
        var func = async () => await this.SUT.PlaceLimitOrderAsync(OrderSide.Sell, limitPrice, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);

        // Assert
        await func.Should().ThrowExactlyAsync<InternalTradingServiceException>();
    }

    [Test]
    public async Task PlaceLimitOrder_ShouldThrow_WhenTradingStopParametersAreIncorrect()
    {
        // Arrange
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var limitPrice = lastPrice + 500;
        var stopLoss = limitPrice - 300;
        var takeProfit = limitPrice + 300;
        var tradingStopTriggerType = TriggerType.LastPrice;

        // Act
        var func = async () => await this.SUT.PlaceLimitOrderAsync(OrderSide.Sell, limitPrice, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);

        // Assert
        await func.Should().ThrowExactlyAsync<InternalTradingServiceException>();
    }

    [Test]
    public async Task PlaceLimitOrder_ShouldThrow_WhenLimitSellOrderAlreadyExists()
    {
        // Arrange
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var limitPrice = lastPrice + 500;
        var stopLoss = limitPrice + 300;
        var takeProfit = limitPrice - 300;
        var tradingStopTriggerType = TriggerType.LastPrice;
        await this.SUT.PlaceLimitOrderAsync(OrderSide.Sell, limitPrice, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);

        // Act
        var func = async () => await this.SUT.PlaceLimitOrderAsync(OrderSide.Sell, limitPrice, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);

        // Assert
        await func.Should().ThrowExactlyAsync<InvalidOrderException>("There is an open Sell limit order already");
    }
}
