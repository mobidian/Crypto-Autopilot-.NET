using Application.Exceptions;

using Bybit.Net.Enums;

using Infrastructure.Tests.Integration.BybitExchange.BybitUsdFuturesTradingServiceTests.AbstractBase;

namespace Infrastructure.Tests.Integration.BybitExchange.BybitUsdFuturesTradingServiceTests.LongPositions;

public class ModifyLimitBuyOrder : BybitUsdFuturesTradingServiceTestsBase
{
    [Test]
    public async Task ModifyLimitOrder_ShouldModifyBuyOrder_WhenBuyOrderExists()
    {
        // Arrange
        var lastPrice = await this.FuturesDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var limitPrice = lastPrice - 1000;
        var stopLoss = limitPrice - 400;
        var takeProfit = limitPrice + 400;
        var tradingStopTriggerType = TriggerType.LastPrice;
        
        await this.SUT.PlaceLimitOrderAsync(OrderSide.Buy, limitPrice, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);

        
        // Act
        var newLimitPrice = limitPrice - 100;
        var newMargin = this.Margin + 50;
        var newStopLoss = newLimitPrice - 400;
        var newTakeProfit = newLimitPrice + 400;
        var newTradingStopTriggerType = TriggerType.MarkPrice;

        await this.SUT.ModifyLimitOrderAsync(OrderSide.Buy, newLimitPrice, newMargin, newStopLoss, newTakeProfit, newTradingStopTriggerType);

        
        // Assert
        this.SUT.BuyLimitOrder.Should().NotBeNull();
        this.SUT.BuyLimitOrder!.Side.Should().Be(OrderSide.Buy);
        this.SUT.BuyLimitOrder!.Price.Should().Be(newLimitPrice);
        this.SUT.BuyLimitOrder!.Quantity.Should().Be(Math.Round(newMargin * this.Leverage / newLimitPrice, 2));
        this.SUT.BuyLimitOrder!.StopLoss.Should().Be(newStopLoss);
        this.SUT.BuyLimitOrder!.StopLossTriggerType.Should().Be(newTradingStopTriggerType);
        this.SUT.BuyLimitOrder!.TakeProfit.Should().Be(newTakeProfit);
        this.SUT.BuyLimitOrder!.TakeProfitTriggerType.Should().Be(newTradingStopTriggerType);
    }
    
    [Test]
    public async Task ModifyLimitOrder_ShouldThrow_WhenNewLimitPriceIsIncorrect()
    {
        // Arrange
        var lastPrice = await this.FuturesDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var limitPrice = lastPrice - 1000;
        var stopLoss = limitPrice - 400;
        var takeProfit = limitPrice + 400;
        var tradingStopTriggerType = TriggerType.LastPrice;

        await this.SUT.PlaceLimitOrderAsync(OrderSide.Buy, limitPrice, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);


        // Act
        var newLimitPrice = lastPrice + 1000;
        var newMargin = this.Margin + 50;
        var newStopLoss = newLimitPrice - 400;
        var newTakeProfit = newLimitPrice + 400;
        var newTradingStopTriggerType = TriggerType.MarkPrice;

        var func = async () => await this.SUT.ModifyLimitOrderAsync(OrderSide.Buy, newLimitPrice, newMargin, newStopLoss, newTakeProfit, newTradingStopTriggerType);

               
        // Assert
        await func.Should().ThrowExactlyAsync<InternalTradingServiceException>();
    }
    
    [Test]
    public async Task ModifyLimitOrder_ShouldThrow_WhenNewTradingStopParametersAreIncorrect()
    {
        // Arrange
        var lastPrice = await this.FuturesDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var limitPrice = lastPrice - 1000;
        var stopLoss = limitPrice - 400;
        var takeProfit = limitPrice + 400;
        var tradingStopTriggerType = TriggerType.LastPrice;

        await this.SUT.PlaceLimitOrderAsync(OrderSide.Buy, limitPrice, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);


        // Act
        var newLimitPrice = limitPrice - 100;
        var newMargin = this.Margin + 50;
        var newStopLoss = newLimitPrice + 400;
        var newTakeProfit = newLimitPrice - 400;
        var newTradingStopTriggerType = TriggerType.MarkPrice;

        var func = async () => await this.SUT.ModifyLimitOrderAsync(OrderSide.Buy, newLimitPrice, newMargin, newStopLoss, newTakeProfit, newTradingStopTriggerType);


        // Assert
        await func.Should().ThrowExactlyAsync<InternalTradingServiceException>();
    }

    [Test]
    public async Task ModifyLimitOrder_ShouldThrow_WhenLimitBuyOrderDoesNotExist()
    {
        // Arrange
        var lastPrice = await this.FuturesDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var limitPrice = lastPrice - 500;
        var stopLoss = limitPrice - 300;
        var takeProfit = limitPrice + 300;
        var tradingStopTriggerType = TriggerType.LastPrice;
        
        // Act
        var func = async () => await this.SUT.ModifyLimitOrderAsync(OrderSide.Buy, limitPrice, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);
        
        // Assert
        await func.Should().ThrowExactlyAsync<InvalidOrderException>("No open {orderSide} limit order was found");
    }
}
