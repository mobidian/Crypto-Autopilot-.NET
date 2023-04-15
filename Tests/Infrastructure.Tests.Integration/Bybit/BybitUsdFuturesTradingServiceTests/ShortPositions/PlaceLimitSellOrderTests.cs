using Application.Exceptions;

using Bybit.Net.Enums;

using Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.AbstractBase;

namespace Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.ShortPositions;

public class PlaceLimitSellOrderTests : BybitUsdFuturesTradingServiceTestsBase
{
    [TestCase(300, -300, Description = "Both StopLoss and TakeProfit specified")]
    [TestCase(300, null, Description = "Only StopLoss specified")]
    [TestCase(null, -300, Description = "Only TakeProfit specified")]
    [TestCase(null, null, Description = "Neither StopLoss nor TakeProfit specified")]
    public async Task PlaceLimitOrder_ShouldPlaceSellLimitOrder_WhenNoSellLimitOrderExists(int? stopLossOffset, int? takeProfitOffset)
    {
        // Arrange
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var limitPrice = lastPrice + 500;
        decimal? stopLoss = stopLossOffset.HasValue ? limitPrice + stopLossOffset.Value : null;
        decimal? takeProfit = takeProfitOffset.HasValue ? limitPrice + takeProfitOffset.Value : null;
        var tradingStopTriggerType = TriggerType.LastPrice;

        // Act
        await this.SUT.PlaceLimitOrderAsync(OrderSide.Sell, limitPrice, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);
        
        // Assert
        this.SUT.SellLimitOrders.Should().NotBeNullOrEmpty();
        this.SUT.SellLimitOrders.Single().Side.Should().Be(OrderSide.Sell);
        this.SUT.SellLimitOrders.Single().Price.Should().Be(limitPrice);
        this.SUT.SellLimitOrders.Single().Quantity.Should().Be(Math.Round(this.Margin * this.Leverage / limitPrice, 2));
        this.SUT.SellLimitOrders.Single().StopLoss.Should().Be(stopLossOffset.HasValue ? stopLoss!.Value : 0);
        this.SUT.SellLimitOrders.Single().TakeProfit.Should().Be(takeProfitOffset.HasValue ? takeProfit!.Value : 0);

        var order = await this.TradingClient.GetOrderAsync(this.CurrencyPair.Name, this.SUT.SellLimitOrders.Single().BybitID.ToString());
        order.Side.Should().Be(OrderSide.Sell);
        order.Price.Should().Be(limitPrice);
        order.Quantity.Should().Be(Math.Round(this.Margin * this.Leverage / limitPrice, 2));
        order.StopLoss.Should().Be(stopLossOffset.HasValue ? stopLoss!.Value : 0);
        order.StopLossTriggerType.Should().Be(stopLossOffset.HasValue ? tradingStopTriggerType : TriggerType.Unknown);
        order.TakeProfit.Should().Be(takeProfitOffset.HasValue ? takeProfit!.Value : 0);
        order.TakeProfitTriggerType.Should().Be(takeProfitOffset.HasValue ? tradingStopTriggerType : TriggerType.Unknown);
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
}
