using Application.Exceptions;

using Bybit.Net.Enums;

using Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.AbstractBase;

namespace Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.ShortPositions;

public class ModifyTradingStopOnShort : BybitUsdFuturesTradingServiceTestsBase
{
    [TestCase(-100, 100, Description = "Both StopLoss and TakeProfit specified")]
    [TestCase(-100, 0, Description = "Only StopLoss specified")]
    [TestCase(0, 100, Description = "Only TakeProfit specified")]
    public async Task ModifyTradingStop_ShouldModifyTradingStop_WhenShortPositionExists(int newStopLossOffset, int newTakeProfitOffset)
    {
        // Arrange
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var stopLoss = lastPrice + 300;
        var takeProfit = lastPrice - 300;
        var tradingStopTriggerType = TriggerType.LastPrice;

        await this.SUT.OpenPositionAsync(PositionSide.Sell, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);


        // Act
        decimal? newStopLoss = stopLoss + newStopLossOffset;
        decimal? newTakeProfit = takeProfit + newTakeProfitOffset;

        await this.SUT.ModifyTradingStopAsync(PositionSide.Sell, newStopLoss, newTakeProfit, tradingStopTriggerType);


        // Assert
        this.SUT.ShortPosition.Should().NotBeNull();
        this.SUT.ShortPosition!.StopLoss.Should().Be(newStopLoss);
        this.SUT.ShortPosition!.TakeProfit.Should().Be(newTakeProfit);
    }

    [Test]
    public async Task ModifyTradingStop_ShouldThrow_WhenShortPositionDoesNotExist()
    {
        // Arrange
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var stopLoss = lastPrice + 300;
        var takeProfit = lastPrice - 300;
        var tradingStopTriggerType = TriggerType.LastPrice;

        // Act
        var func = async () => await this.SUT.ModifyTradingStopAsync(PositionSide.Sell, stopLoss, takeProfit, tradingStopTriggerType);

        // Assert
        await func.Should().ThrowExactlyAsync<InvalidOrderException>("No open Sell was found");
    }
}
