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
        this.SUT.SellLimitOrder.Should().NotBeNull();
        this.SUT.SellLimitOrder!.Side.Should().Be(OrderSide.Sell);
        this.SUT.SellLimitOrder!.Price.Should().Be(limitPrice);
        this.SUT.SellLimitOrder!.Quantity.Should().Be(Math.Round(this.Margin * this.Leverage / limitPrice, 2));
        this.SUT.SellLimitOrder!.StopLoss.Should().Be(stopLossOffset.HasValue ? stopLoss!.Value : 0);
        this.SUT.SellLimitOrder!.StopLossTriggerType.Should().Be(stopLossOffset.HasValue ? tradingStopTriggerType : TriggerType.Unknown);
        this.SUT.SellLimitOrder!.TakeProfit.Should().Be(takeProfitOffset.HasValue ? takeProfit!.Value : 0);
        this.SUT.SellLimitOrder!.TakeProfitTriggerType.Should().Be(takeProfitOffset.HasValue ? tradingStopTriggerType : TriggerType.Unknown);
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
