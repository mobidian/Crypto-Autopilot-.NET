using Application.Exceptions;

using Bybit.Net.Enums;

using Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.AbstractBase;

namespace Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.LongPositions;

public class PlaceLimitBuyOrderTests : BybitUsdFuturesTradingServiceTestsBase
{
    [TestCase(-300, 300, Description = "Both StopLoss and TakeProfit specified")]
    [TestCase(-300, null, Description = "Only StopLoss specified")]
    [TestCase(null, 300, Description = "Only TakeProfit specified")]
    [TestCase(null, null, Description = "Neither StopLoss nor TakeProfit specified")]
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
        this.SUT.BuyLimitOrders.Single().StopLossTriggerType.Should().Be(stopLossOffset.HasValue ? tradingStopTriggerType : TriggerType.Unknown);
        this.SUT.BuyLimitOrders.Single().TakeProfit.Should().Be(takeProfitOffset.HasValue ? takeProfit!.Value : 0);
        this.SUT.BuyLimitOrders.Single().TakeProfitTriggerType.Should().Be(takeProfitOffset.HasValue ? tradingStopTriggerType : TriggerType.Unknown);
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
}
