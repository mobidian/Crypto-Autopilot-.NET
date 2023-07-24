using Application.Exceptions;

using Bybit.Net.Enums;

using FluentAssertions;

using Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.AbstractBase;

using Tests.Integration.Common.Fixtures;

using Xunit;

namespace Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.LongPositions;

public class PlaceLimitBuyOrderTests : BybitUsdFuturesTradingServiceTestsBase
{
    public PlaceLimitBuyOrderTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }


    [Theory]
    [InlineData(-300, 300)] // Both StopLoss and TakeProfit specified
    [InlineData(-300, null)] // Only StopLoss specified
    [InlineData(null, 300)] // Only TakeProfit specified
    [InlineData(null, null)] // Neither StopLoss nor TakeProfit specified
    public async Task PlaceLimitOrder_ShouldPlaceBuyLimitOrder_WhenNoBuyLimitOrderExists(int? stopLossOffset, int? takeProfitOffset)
    {
        // Arrange
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var limitPrice = lastPrice - 500;
        decimal? stopLoss = stopLossOffset.HasValue ? limitPrice + stopLossOffset.Value : null;
        decimal? takeProfit = takeProfitOffset.HasValue ? limitPrice + takeProfitOffset.Value : null;
        var tradingStopTriggerType = TriggerType.LastPrice;

        // Act
        await this.SUT.PlaceLimitOrderAsync(OrderSide.Buy, limitPrice, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);
        
        // Assert
        this.SUT.BuyLimitOrders.Should().NotBeNullOrEmpty();
        this.SUT.BuyLimitOrders.Single().Side.Should().Be(OrderSide.Buy);
        this.SUT.BuyLimitOrders.Single().Price.Should().Be(limitPrice);
        this.SUT.BuyLimitOrders.Single().Quantity.Should().Be(Math.Round(this.Margin * this.Leverage / limitPrice, 2));
        this.SUT.BuyLimitOrders.Single().StopLoss.Should().Be(stopLossOffset.HasValue ? stopLoss!.Value : 0);
        this.SUT.BuyLimitOrders.Single().TakeProfit.Should().Be(takeProfitOffset.HasValue ? takeProfit!.Value : 0);
        
        var order = await this.TradingClient.GetOrderAsync(this.CurrencyPair.Name, this.SUT.BuyLimitOrders.Single().BybitID);
        order.Side.Should().Be(OrderSide.Buy);
        order.Price.Should().Be(limitPrice);
        order.Quantity.Should().Be(Math.Round(this.Margin * this.Leverage / limitPrice, 2));
        order.StopLoss.Should().Be(stopLossOffset.HasValue ? stopLoss!.Value : 0);
        order.StopLossTriggerType.Should().Be(stopLossOffset.HasValue ? tradingStopTriggerType : TriggerType.Unknown);
        order.TakeProfit.Should().Be(takeProfitOffset.HasValue ? takeProfit!.Value : 0);
        order.TakeProfitTriggerType.Should().Be(takeProfitOffset.HasValue ? tradingStopTriggerType : TriggerType.Unknown);
    }

    [Fact]
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

    [Fact]
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
}
