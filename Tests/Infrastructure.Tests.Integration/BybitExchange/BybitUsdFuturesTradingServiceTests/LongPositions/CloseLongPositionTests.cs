using Application.Exceptions;

using Bybit.Net.Enums;

using Infrastructure.Tests.Integration.BybitExchange.BybitUsdFuturesTradingServiceTests.AbstractBase;

namespace Infrastructure.Tests.Integration.BybitExchange.BybitUsdFuturesTradingServiceTests.LongPositions;

public class CloseLongPositionTests : BybitUsdFuturesTradingServiceTestsBase
{
    [Test]
    public async Task ClosePosition_ShouldCloseLongPosition_WhenLongPositionExists()
    {
        // Arrange
        var lastPrice = await this.FuturesDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var stopLoss = lastPrice - 300;
        var takeProfit = lastPrice + 300;
        var tradingStopTriggerType = TriggerType.LastPrice;

        await this.SUT.OpenPositionAsync(PositionSide.Buy, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);

        // Act
        await this.SUT.ClosePositionAsync(PositionSide.Buy);
        
        // Assert
        this.SUT.LongPosition.Should().BeNull();
    }
     
    [Test]
    public async Task ClosePosition_ShouldThrow_WhenLongPositionDoesNotExist()
    {
        // Act
        var func = async () => await this.SUT.ClosePositionAsync(PositionSide.Buy);
        
        // Assert
        await func.Should().ThrowExactlyAsync<InvalidOrderException>("No open Buy position was found");
    }
}
