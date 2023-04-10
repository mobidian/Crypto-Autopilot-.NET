using Application.Exceptions;

using Bybit.Net.Enums;

using Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.AbstractBase;

namespace Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.ShortPositions;

public class OpenShortPositionTests : BybitUsdFuturesTradingServiceTestsBase
{
    [TestCase(300, -300, Description = "Both StopLoss and TakeProfit specified")]
    [TestCase(300, null, Description = "Only StopLoss specified")]
    [TestCase(null, -300, Description = "Only TakeProfit specified")]
    [TestCase(null, null, Description = "Neither StopLoss nor TakeProfit specified")]
    public async Task OpenPosition_ShouldOpenShortPosition_WhenShortPositionDoesNotExist(int? stopLossOffset, int? takeProfitOffset)
    {
        // Arrange
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        decimal? stopLoss = stopLossOffset.HasValue ? lastPrice + stopLossOffset.Value : null;
        decimal? takeProfit = takeProfitOffset.HasValue ? lastPrice + takeProfitOffset.Value : null;
        var tradingStopTriggerType = TriggerType.LastPrice;
        
        // Act
        await this.SUT.OpenPositionAsync(PositionSide.Sell, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);
        
        // Assert
        this.SUT.ShortPosition.Should().NotBeNull();
        this.SUT.ShortPosition!.Side.Should().Be(PositionSide.Sell);
        this.SUT.ShortPosition!.PositionMode.Should().Be(PositionMode.BothSideSell);
        this.SUT.ShortPosition!.Leverage.Should().Be(this.Leverage);
        this.SUT.ShortPosition!.StopLoss.Should().Be(stopLossOffset.HasValue ? stopLoss!.Value : 0);
        this.SUT.ShortPosition!.TakeProfit.Should().Be(takeProfitOffset.HasValue ? takeProfit!.Value : 0);
        this.SUT.ShortPosition!.StopLossTakeProfitMode.Should().Be(StopLossTakeProfitMode.Full);
        
        var position = await this.FuturesAccount.GetPositionAsync(this.CurrencyPair.Name, PositionSide.Sell);
        position!.Side.Should().Be(PositionSide.Sell);
        position!.PositionMode.Should().Be(PositionMode.BothSideSell);
        position!.Leverage.Should().Be(this.Leverage);
        position!.StopLoss.Should().Be(stopLossOffset.HasValue ? stopLoss!.Value : 0);
        position!.TakeProfit.Should().Be(takeProfitOffset.HasValue ? takeProfit!.Value : 0);
        position!.StopLossTakeProfitMode.Should().Be(StopLossTakeProfitMode.Full);
    }

    [Test]
    public async Task OpenPosition_ShouldThrow_WhenTradingStopParametersAreIncorrect()
    {
        // Arrange
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var stopLoss = lastPrice - 300;
        var takeProfit = lastPrice + 300;
        var tradingStopTriggerType = TriggerType.LastPrice;

        // Act
        var func = async () => await this.SUT.OpenPositionAsync(PositionSide.Sell, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);

        // Assert
        await func.Should().ThrowExactlyAsync<InternalTradingServiceException>();
    }
}
