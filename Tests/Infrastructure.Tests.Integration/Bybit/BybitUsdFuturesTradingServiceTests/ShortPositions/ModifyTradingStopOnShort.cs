using Application.Exceptions;

using Bybit.Net.Enums;

using Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.AbstractBase;

namespace Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.ShortPositions;

public class ModifyTradingStopOnShort : BybitUsdFuturesTradingServiceTestsBase
{
    [Test]
    public async Task ModifyTradingStop_ShouldModifyTradingStop_WhenShortPositionExists()
    {
        // Arrange
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var stopLoss = lastPrice + 300;
        var takeProfit = lastPrice - 300;
        var tradingStopTriggerType = TriggerType.LastPrice;

        await this.SUT.OpenPositionAsync(PositionSide.Sell, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);


        // Act
        var newStopLoss = stopLoss - 100;
        var newTakeProfit = takeProfit - 100;

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
