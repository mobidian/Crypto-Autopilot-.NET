﻿using Application.Exceptions;

using Bybit.Net.Enums;

using Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.AbstractBase;

namespace Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.LongPositions;

public class ModifyTradingStopOnLong : BybitUsdFuturesTradingServiceTestsBase
{
    [TestCase(100, -100, Description = "Both StopLoss and TakeProfit specified")]
    [TestCase(100, 0, Description = "Only StopLoss specified")]
    [TestCase(0, -100, Description = "Only TakeProfit specified")]
    public async Task ModifyTradingStop_ShouldModifyTradingStop_WhenLongPositionExists(int newStopLossOffset, int newTakeProfitOffset)
    {
        // Arrange
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var stopLoss = lastPrice - 300;
        var takeProfit = lastPrice + 300;
        var tradingStopTriggerType = TriggerType.LastPrice;

        await this.SUT.OpenPositionAsync(PositionSide.Buy, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);
        

        // Act
        decimal? newStopLoss = stopLoss + newStopLossOffset;
        decimal? newTakeProfit = takeProfit + newTakeProfitOffset;

        await this.SUT.ModifyTradingStopAsync(PositionSide.Buy, newStopLoss, newTakeProfit, tradingStopTriggerType);

        
        // Assert
        this.SUT.LongPosition.Should().NotBeNull();
        this.SUT.LongPosition!.StopLoss.Should().Be(newStopLoss);
        this.SUT.LongPosition!.TakeProfit.Should().Be(newTakeProfit);
    }

    [Test]
    public async Task ModifyTradingStop_ShouldThrow_WhenLongPositionDoesNotExist()
    {
        // Arrange
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var stopLoss = lastPrice - 300;
        var takeProfit = lastPrice + 300;
        var tradingStopTriggerType = TriggerType.LastPrice;

        // Act
        var func = async () => await this.SUT.ModifyTradingStopAsync(PositionSide.Buy, stopLoss, takeProfit, tradingStopTriggerType);

        // Assert
        await func.Should().ThrowExactlyAsync<InvalidOrderException>("No open Buy was found");
    }
}
