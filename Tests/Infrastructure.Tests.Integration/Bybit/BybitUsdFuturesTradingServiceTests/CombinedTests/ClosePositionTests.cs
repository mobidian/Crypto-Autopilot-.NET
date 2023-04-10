using Application.Exceptions;

using Bybit.Net.Enums;

using Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.AbstractBase;

namespace Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.CombinedTests;

public class ClosePositionTests : BybitUsdFuturesTradingServiceTestsBase
{
    [TestCase(PositionSide.Buy, -300, 300)]
    [TestCase(PositionSide.Sell, 300, -300)]
    public async Task ClosePosition_ShouldClosePosition_WhenPositionExists(PositionSide positionSide, decimal stopLossOffset, decimal takeProfitOffset)
    {
        // Arrange
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var stopLoss = lastPrice + stopLossOffset;
        var takeProfit = lastPrice + takeProfitOffset;
        var tradingStopTriggerType = TriggerType.LastPrice;
        
        await this.SUT.OpenPositionAsync(positionSide, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);


        // Act
        await this.SUT.ClosePositionAsync(positionSide);


        // Assert
        var position = positionSide == PositionSide.Buy ? this.SUT.LongPosition : this.SUT.ShortPosition;
        position.Should().BeNull();
    }
    
    [TestCase(PositionSide.Buy, "No open Buy position was found")]
    [TestCase(PositionSide.Sell, "No open Sell position was found")]
    public async Task ClosePosition_ShouldThrow_WhenPositionDoesNotExist(PositionSide positionSide, string expectedErrorMessage)
    {
        // Act
        var func = async () => await this.SUT.ClosePositionAsync(positionSide);

        // Assert
        await func.Should().ThrowExactlyAsync<InvalidOrderException>(expectedErrorMessage);
    }
    
    [Test]
    public async Task CloseAllPositionsAsync_ShouldCloseAllOpenPositions()
    {
        // Arrange
        await this.SUT.OpenPositionAsync(PositionSide.Buy, this.Margin);
        await this.SUT.OpenPositionAsync(PositionSide.Sell, this.Margin);

        // Act
        await this.SUT.CloseAllPositionsAsync();

        // Assert
        this.SUT.LongPosition.Should().BeNull();
        this.SUT.ShortPosition.Should().BeNull();
    }
}
