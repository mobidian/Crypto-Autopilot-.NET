using Application.Exceptions;

using Bybit.Net.Enums;

using Infrastructure.Tests.Integration.BybitExchange.BybitUsdFuturesTradingServiceTests.AbstractBase;

namespace Infrastructure.Tests.Integration.BybitExchange.BybitUsdFuturesTradingServiceTests.ShortPositions;

public class CloseShortPosition : BybitUsdFuturesTradingServiceTestsBase
{
    [Test]
    public async Task ClosePosition_ShouldCloseShortPosition_WhenShortPositionExists()
    {
        // Arrange
        var lastPrice = await this.FuturesDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var stopLoss = lastPrice + 300;
        var takeProfit = lastPrice - 300;
        var tradingStopTriggerType = TriggerType.LastPrice;

        await this.SUT.OpenPositionAsync(PositionSide.Sell, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);
        
        // Act
        await this.SUT.ClosePositionAsync(PositionSide.Sell);
        
        // Assert
        this.SUT.ShortPosition.Should().BeNull();
    }

    [Test]
    public async Task ClosePosition_ShouldThrow_WhenShortPositionDoesNotExist()
    {
        // Act
        var func = async () => await this.SUT.ClosePositionAsync(PositionSide.Sell);

        // Assert
        await func.Should().ThrowExactlyAsync<InvalidOrderException>("No open Sell position was found");
    }
}
