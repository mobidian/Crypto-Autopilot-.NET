using Application.Exceptions;

using Bybit.Net.Enums;

using Infrastructure.Tests.Integration.BybitExchange.BybitUsdFuturesTradingServiceTests.AbstractBase;

namespace Infrastructure.Tests.Integration.BybitExchange.BybitUsdFuturesTradingServiceTests.ShortPositions;

public class ModifyLimitSellOrder : BybitUsdFuturesTradingServiceTestsBase
{
    [Test]
    public async Task ModifyLimitOrder_ShouldModifySellOrder_WhenSellOrderExists()
    {
        // Arrange
        var lastPrice = await this.FuturesDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var limitPrice = lastPrice + 1000;
        var stopLoss = limitPrice + 400;
        var takeProfit = limitPrice - 400;
        var tradingStopTriggerType = TriggerType.LastPrice;

        await this.SUT.PlaceLimitOrderAsync(OrderSide.Sell, limitPrice, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);
        

        // Act
        var newLimitPrice = limitPrice + 100;
        var newMargin = this.Margin + 50;
        var newStopLoss = newLimitPrice + 400;
        var newTakeProfit = newLimitPrice - 400;
        var newTradingStopTriggerType = TriggerType.MarkPrice;
        
        await this.SUT.ModifyLimitOrderAsync(OrderSide.Sell, newLimitPrice, newMargin, newStopLoss, newTakeProfit, newTradingStopTriggerType);
        

        // Assert
        this.SUT.SellLimitOrder.Should().NotBeNull();
        this.SUT.SellLimitOrder!.Side.Should().Be(OrderSide.Sell);
        this.SUT.SellLimitOrder!.Price.Should().Be(newLimitPrice);
        this.SUT.SellLimitOrder!.Quantity.Should().Be(Math.Round(newMargin * this.Leverage / newLimitPrice, 2));
        this.SUT.SellLimitOrder!.StopLoss.Should().Be(newStopLoss);
        this.SUT.SellLimitOrder!.StopLossTriggerType.Should().Be(newTradingStopTriggerType);
        this.SUT.SellLimitOrder!.TakeProfit.Should().Be(newTakeProfit);
        this.SUT.SellLimitOrder!.TakeProfitTriggerType.Should().Be(newTradingStopTriggerType);
    }

    [Test]
    public async Task ModifyLimitOrder_ShouldThrow_WhenNewLimitPriceIsIncorrect()
    {
        // Arrange
        var lastPrice = await this.FuturesDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var limitPrice = lastPrice + 1000;
        var stopLoss = limitPrice + 400;
        var takeProfit = limitPrice - 400;
        var tradingStopTriggerType = TriggerType.LastPrice;

        await this.SUT.PlaceLimitOrderAsync(OrderSide.Sell, limitPrice, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);


        // Act
        var newLimitPrice = lastPrice - 1000;
        var newMargin = this.Margin + 50;
        var newStopLoss = newLimitPrice + 400;
        var newTakeProfit = newLimitPrice - 400;
        var newTradingStopTriggerType = TriggerType.MarkPrice;

        var func = async () => await this.SUT.ModifyLimitOrderAsync(OrderSide.Sell, newLimitPrice, newMargin, newStopLoss, newTakeProfit, newTradingStopTriggerType);


        // Assert
        await func.Should().ThrowExactlyAsync<InternalTradingServiceException>();
    }

    [Test]
    public async Task ModifyLimitOrder_ShouldThrow_WhenNewTradingStopParametersAreIncorrect()
    {
        // Arrange
        var lastPrice = await this.FuturesDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var limitPrice = lastPrice + 1000;
        var stopLoss = limitPrice + 400;
        var takeProfit = limitPrice - 400;
        var tradingStopTriggerType = TriggerType.LastPrice;

        await this.SUT.PlaceLimitOrderAsync(OrderSide.Sell, limitPrice, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);

        
        // Act
        var newLimitPrice = limitPrice + 100;
        var newMargin = this.Margin + 50;
        var newStopLoss = newLimitPrice - 400;
        var newTakeProfit = newLimitPrice + 400;
        var newTradingStopTriggerType = TriggerType.MarkPrice;

        var func = async () => await this.SUT.ModifyLimitOrderAsync(OrderSide.Sell, newLimitPrice, newMargin, newStopLoss, newTakeProfit, newTradingStopTriggerType);

        
        // Assert
        await func.Should().ThrowExactlyAsync<InternalTradingServiceException>();
    }

    [Test]
    public async Task ModifyLimitOrder_ShouldThrow_WhenLimitSellOrderDoesNotExist()
    {
        // Arrange
        var lastPrice = await this.FuturesDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var limitPrice = lastPrice + 1000;
        var stopLoss = limitPrice + 400;
        var takeProfit = limitPrice - 400;
        var tradingStopTriggerType = TriggerType.LastPrice;

        // Act
        var func = async () => await this.SUT.ModifyLimitOrderAsync(OrderSide.Sell, limitPrice, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);
        
        // Assert
        await func.Should().ThrowExactlyAsync<InvalidOrderException>();
    }
}
