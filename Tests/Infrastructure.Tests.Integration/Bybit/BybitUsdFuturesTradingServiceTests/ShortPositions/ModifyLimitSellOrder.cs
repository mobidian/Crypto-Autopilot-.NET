using Application.Exceptions;

using Bybit.Net.Enums;

using CryptoExchange.Net.CommonObjects;

using Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.AbstractBase;

namespace Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.ShortPositions;

public class ModifyLimitSellOrder : BybitUsdFuturesTradingServiceTestsBase
{
    [Test]
    public async Task ModifyLimitOrder_ShouldModifySellOrder_WhenSellOrderExists()
    {
        // Arrange
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var limitPrice = lastPrice + 1000;
        var stopLoss = limitPrice + 400;
        var takeProfit = limitPrice - 400;
        var tradingStopTriggerType = TriggerType.LastPrice;

        var order = await this.SUT.PlaceLimitOrderAsync(OrderSide.Sell, limitPrice, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);


        // Act
        var newLimitPrice = limitPrice + 100;
        var newMargin = this.Margin + 50;
        var newStopLoss = newLimitPrice + 400;
        var newTakeProfit = newLimitPrice - 400;
        var newTradingStopTriggerType = TriggerType.MarkPrice;

        var orderId = Guid.Parse(order.Id);
        await this.SUT.ModifyLimitOrderAsync(orderId, newLimitPrice, newMargin, newStopLoss, newTakeProfit, newTradingStopTriggerType);

        
        // Assert
        this.SUT.SellLimitOrders.Should().NotBeNullOrEmpty();
        this.SUT.SellLimitOrders.Single().Side.Should().Be(OrderSide.Sell);
        this.SUT.SellLimitOrders.Single().Price.Should().Be(newLimitPrice);
        this.SUT.SellLimitOrders.Single().Quantity.Should().Be(Math.Round(newMargin * this.Leverage / newLimitPrice, 2));
        this.SUT.SellLimitOrders.Single().StopLoss.Should().Be(newStopLoss);
        this.SUT.SellLimitOrders.Single().StopLossTriggerType.Should().Be(newTradingStopTriggerType);
        this.SUT.SellLimitOrders.Single().TakeProfit.Should().Be(newTakeProfit);
        this.SUT.SellLimitOrders.Single().TakeProfitTriggerType.Should().Be(newTradingStopTriggerType);
    }

    [Test]
    public async Task ModifyLimitOrder_ShouldThrow_WhenNewLimitPriceIsIncorrect()
    {
        // Arrange
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var limitPrice = lastPrice + 1000;
        var stopLoss = limitPrice + 400;
        var takeProfit = limitPrice - 400;
        var tradingStopTriggerType = TriggerType.LastPrice;

        var order = await this.SUT.PlaceLimitOrderAsync(OrderSide.Sell, limitPrice, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);


        // Act
        var newLimitPrice = lastPrice - 1000;
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
    public async Task ModifyLimitOrder_ShouldThrow_WhenNewTradingStopParametersAreIncorrect()
    {
        // Arrange
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var limitPrice = lastPrice + 1000;
        var stopLoss = limitPrice + 400;
        var takeProfit = limitPrice - 400;
        var tradingStopTriggerType = TriggerType.LastPrice;
        
        var order = await this.SUT.PlaceLimitOrderAsync(OrderSide.Sell, limitPrice, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);


        // Act
        var newLimitPrice = limitPrice + 100;
        var newMargin = this.Margin + 50;
        var newStopLoss = newLimitPrice - 400;
        var newTakeProfit = newLimitPrice + 400;
        var newTradingStopTriggerType = TriggerType.MarkPrice;

        var orderId = Guid.Parse(order.Id);
        var func = async () => await this.SUT.ModifyLimitOrderAsync(Guid.NewGuid(), newLimitPrice, newMargin, newStopLoss, newTakeProfit, newTradingStopTriggerType);

        
        // Assert
        await func.Should().ThrowExactlyAsync<InvalidOrderException>();
    }

    [Test]
    public async Task ModifyLimitOrder_ShouldThrow_WhenLimitSellOrderDoesNotExist()
    {
        // Arrange
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var limitPrice = lastPrice + 1000;
        var stopLoss = limitPrice + 400;
        var takeProfit = limitPrice - 400;
        var tradingStopTriggerType = TriggerType.LastPrice;

        // Act
        var orderId = Guid.NewGuid();
        var func = async () => await this.SUT.ModifyLimitOrderAsync(orderId, limitPrice, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);
        
        // Assert
        await func.Should().ThrowExactlyAsync<InvalidOrderException>($"No open limit order with id == {orderId} was found");
    }
}
