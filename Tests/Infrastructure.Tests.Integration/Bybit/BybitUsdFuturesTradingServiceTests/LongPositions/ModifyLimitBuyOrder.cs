using Application.Exceptions;

using Bybit.Net.Enums;

using FluentAssertions;

using Infrastructure.Tests.Integration.AbstractBases;
using Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.AbstractBase;

using Xunit;

namespace Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.LongPositions;

public class ModifyLimitBuyOrder : BybitUsdFuturesTradingServiceTestsBase
{
    public ModifyLimitBuyOrder(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }


    [Fact]
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
        
        await this.SUT.ModifyLimitOrderAsync(order.BybitID, newLimitPrice, newMargin, newStopLoss, newTakeProfit, newTradingStopTriggerType);


        // Assert
        this.SUT.BuyLimitOrders.Should().NotBeNullOrEmpty();
        this.SUT.BuyLimitOrders.Single().Side.Should().Be(OrderSide.Buy);
        this.SUT.BuyLimitOrders.Single().Price.Should().Be(newLimitPrice);
        this.SUT.BuyLimitOrders.Single().Quantity.Should().Be(Math.Round(newMargin * this.Leverage / newLimitPrice, 2));
        this.SUT.BuyLimitOrders.Single().StopLoss.Should().Be(newStopLoss);
        this.SUT.BuyLimitOrders.Single().TakeProfit.Should().Be(newTakeProfit);
        
        var orderFromApi = await this.TradingClient.GetOrderAsync(this.CurrencyPair.Name, this.SUT.BuyLimitOrders.Single().BybitID.ToString());
        orderFromApi.Side.Should().Be(OrderSide.Buy);
        orderFromApi.Price.Should().Be(newLimitPrice);
        orderFromApi.Quantity.Should().Be(Math.Round(newMargin * this.Leverage / newLimitPrice, 2));
        orderFromApi.StopLoss.Should().Be(newStopLoss);
        orderFromApi.StopLossTriggerType.Should().Be(newTradingStopTriggerType);
        orderFromApi.TakeProfit.Should().Be(newTakeProfit);
        orderFromApi.TakeProfitTriggerType.Should().Be(newTradingStopTriggerType);
    }

    [Fact]
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

        var func = async () => await this.SUT.ModifyLimitOrderAsync(order.BybitID, newLimitPrice, newMargin, newStopLoss, newTakeProfit, newTradingStopTriggerType);


        // Assert
        await func.Should().ThrowExactlyAsync<InternalTradingServiceException>();
    }

    [Fact]
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

        var func = async () => await this.SUT.ModifyLimitOrderAsync(order.BybitID, newLimitPrice, newMargin, newStopLoss, newTakeProfit, newTradingStopTriggerType);


        // Assert
        await func.Should().ThrowExactlyAsync<InternalTradingServiceException>();
    }

    [Fact]
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
