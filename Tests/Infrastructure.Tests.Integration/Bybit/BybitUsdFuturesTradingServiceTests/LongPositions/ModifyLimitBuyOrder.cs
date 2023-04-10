using Application.Exceptions;

using Bybit.Net.Enums;
using Bybit.Net.Objects.Models;

using CryptoExchange.Net.CommonObjects;

using Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.AbstractBase;

namespace Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.LongPositions;

public class ModifyLimitBuyOrder : BybitUsdFuturesTradingServiceTestsBase
{
    [Test]
    public async Task ModifyLimitOrder_ShouldModifyBuyOrder_WhenBuyOrderExists()
    {
        // Arrange
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var limitPrice = lastPrice - 1000;
        var stopLoss = limitPrice - 400;
        var takeProfit = limitPrice + 400;
        var tradingStopTriggerType = TriggerType.LastPrice;

        var order = await this.SUT.PlaceLimitOrderAsync(OrderSide.Buy, limitPrice, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);


        // Act
        var newLimitPrice = limitPrice - 100;
        var newMargin = this.Margin + 50;
        var newStopLoss = newLimitPrice - 400;
        var newTakeProfit = newLimitPrice + 400;
        var newTradingStopTriggerType = TriggerType.MarkPrice;
        
        var orderId = Guid.Parse(order.Id);
        await this.SUT.ModifyLimitOrderAsync(orderId, newLimitPrice, newMargin, newStopLoss, newTakeProfit, newTradingStopTriggerType);


        // Assert
        this.SUT.BuyLimitOrders.Should().NotBeNullOrEmpty();
        this.SUT.BuyLimitOrders.Single().Side.Should().Be(OrderSide.Buy);
        this.SUT.BuyLimitOrders.Single().Price.Should().Be(newLimitPrice);
        this.SUT.BuyLimitOrders.Single().Quantity.Should().Be(Math.Round(newMargin * this.Leverage / newLimitPrice, 2));
        this.SUT.BuyLimitOrders.Single().StopLoss.Should().Be(newStopLoss);
        this.SUT.BuyLimitOrders.Single().StopLossTriggerType.Should().Be(newTradingStopTriggerType);
        this.SUT.BuyLimitOrders.Single().TakeProfit.Should().Be(newTakeProfit);
        this.SUT.BuyLimitOrders.Single().TakeProfitTriggerType.Should().Be(newTradingStopTriggerType);
    }

    [Test]
    public async Task ModifyLimitOrder_ShouldThrow_WhenNewLimitPriceIsIncorrect()
    {
        // Arrange
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var limitPrice = lastPrice - 1000;
        var stopLoss = limitPrice - 400;
        var takeProfit = limitPrice + 400;
        var tradingStopTriggerType = TriggerType.LastPrice;

        var order = await this.SUT.PlaceLimitOrderAsync(OrderSide.Buy, limitPrice, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);


        // Act
        var newLimitPrice = lastPrice + 1000;
        var newMargin = this.Margin + 50;
        var newStopLoss = newLimitPrice - 400;
        var newTakeProfit = newLimitPrice + 400;
        var newTradingStopTriggerType = TriggerType.MarkPrice;

        var orderId = Guid.Parse(order.Id);
        var func = async () => await this.SUT.ModifyLimitOrderAsync(orderId, newLimitPrice, newMargin, newStopLoss, newTakeProfit, newTradingStopTriggerType);


        // Assert
        await func.Should().ThrowExactlyAsync<InternalTradingServiceException>();
    }

    [Test]
    public async Task ModifyLimitOrder_ShouldThrow_WhenNewTradingStopParametersAreIncorrect()
    {
        // Arrange
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var limitPrice = lastPrice - 1000;
        var stopLoss = limitPrice - 400;
        var takeProfit = limitPrice + 400;
        var tradingStopTriggerType = TriggerType.LastPrice;

        var order = await this.SUT.PlaceLimitOrderAsync(OrderSide.Buy, limitPrice, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);


        // Act
        var newLimitPrice = limitPrice - 100;
        var newMargin = this.Margin + 50;
        var newStopLoss = newLimitPrice + 400;
        var newTakeProfit = newLimitPrice - 400;
        var newTradingStopTriggerType = TriggerType.MarkPrice;

        var orderId = Guid.Parse(order.Id);
        var func = async () => await this.SUT.ModifyLimitOrderAsync(orderId, newLimitPrice, newMargin, newStopLoss, newTakeProfit, newTradingStopTriggerType);


        // Assert
        await func.Should().ThrowExactlyAsync<InternalTradingServiceException>();
    }

    [Test]
    public async Task ModifyLimitOrder_ShouldThrow_WhenLimitBuyOrderDoesNotExist()
    {
        // Arrange
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var limitPrice = lastPrice - 500;
        var stopLoss = limitPrice - 300;
        var takeProfit = limitPrice + 300;
        var tradingStopTriggerType = TriggerType.LastPrice;

        // Act
        var orderId = Guid.NewGuid();
        var func = async () => await this.SUT.ModifyLimitOrderAsync(orderId, limitPrice, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);

        // Assert
        await func.Should().ThrowExactlyAsync<InvalidOrderException>($"No open limit order with id == {orderId} was found");
    }
}
