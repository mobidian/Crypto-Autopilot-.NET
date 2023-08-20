using Application.Exceptions;

using Bybit.Net.Enums;

using FluentAssertions;

using Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.AbstractBase;

using Tests.Integration.Common.Fixtures;

using Xunit;

namespace Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.ShortPositions;

public class ModifyLimitSellOrder : BybitUsdFuturesTradingServiceTestsBase
{
    public ModifyLimitSellOrder(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }


    [Fact]
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

        await this.SUT.ModifyLimitOrderAsync(order.BybitID, newLimitPrice, newMargin, newStopLoss, newTakeProfit, newTradingStopTriggerType);


        // Assert
        this.SUT.SellLimitOrders.Should().NotBeNullOrEmpty();
        this.SUT.SellLimitOrders.Single().Side.Should().Be(OrderSide.Sell);
        this.SUT.SellLimitOrders.Single().Price.Should().Be(newLimitPrice);
        this.SUT.SellLimitOrders.Single().Quantity.Should().Be(Math.Round(newMargin * this.Leverage / newLimitPrice, 2));
        this.SUT.SellLimitOrders.Single().StopLoss.Should().Be(newStopLoss);
        this.SUT.SellLimitOrders.Single().TakeProfit.Should().Be(newTakeProfit);

        var orderFromApi = await this.TradingClient.GetOrderAsync(this.CurrencyPair.Name, this.SUT.SellLimitOrders.Single().BybitID);
        orderFromApi.Side.Should().Be(OrderSide.Sell);
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

        var func = async () => await this.SUT.ModifyLimitOrderAsync(order.BybitID, newLimitPrice, newMargin, newStopLoss, newTakeProfit, newTradingStopTriggerType);


        // Assert
        await func.Should().ThrowExactlyAsync<InternalTradingServiceException>();
    }

    [Fact]
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

        var func = async () => await this.SUT.ModifyLimitOrderAsync(order.BybitID, newLimitPrice, newMargin, newStopLoss, newTakeProfit, newTradingStopTriggerType);


        // Assert
        await func.Should().ThrowExactlyAsync<InternalTradingServiceException>();
    }

    [Fact]
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
