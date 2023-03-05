using Application.Exceptions;

using Bybit.Net.Enums;

using Infrastructure.Tests.Integration.BybitExchange.BybitUsdFuturesTradingServiceTests.AbstractBase;

namespace Infrastructure.Tests.Integration.BybitExchange.BybitUsdFuturesTradingServiceTests.LongPositions;

public class ModifyTradingStopOnLong : BybitUsdFuturesTradingServiceTestsBase
{
    [Test]
    public async Task ModifyTradingStop_ShouldModifyTradingStop_WhenLongPositionExists()
    {
        // Arrange
        var lastPrice = await this.FuturesDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var stopLoss = lastPrice - 300;
        var takeProfit = lastPrice + 300;
        var tradingStopTriggerType = TriggerType.LastPrice;

        await this.SUT.OpenPositionAsync(PositionSide.Buy, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);

        
        // Act
        var newStopLoss = stopLoss + 100;
        var newTakeProfit = takeProfit + 100;
        
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
        var lastPrice = await this.FuturesDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var stopLoss = lastPrice - 300;
        var takeProfit = lastPrice + 300;
        var tradingStopTriggerType = TriggerType.LastPrice;

        // Act
        var func = async () => await this.SUT.ModifyTradingStopAsync(PositionSide.Buy, stopLoss, takeProfit, tradingStopTriggerType);
        
        // Assert
        await func.Should().ThrowExactlyAsync<InvalidOrderException>("No open Buy was found");
    }
}
