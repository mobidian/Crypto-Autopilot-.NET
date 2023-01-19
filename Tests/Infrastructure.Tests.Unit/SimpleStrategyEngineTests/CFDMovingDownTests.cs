using Binance.Net.Enums;

using Infrastructure.Strategies.SimpleStrategy.Notifications;
using Infrastructure.Tests.Unit.SimpleStrategyEngineTests.Base;

namespace Infrastructure.Tests.Unit.SimpleStrategyEngineTests;

public class CFDMovingDownTests : SimpleStrategyEngineTestsBase
{
    [Test]
    public async Task CFDMovingDown_ShouldTriggerPositionClosing_WhenTraderIsInPosition()
    {
        // Arrange
        this.FuturesTrader.IsInPosition().Returns(true);
        this.FuturesDataProvider.GetCompletedCandlesticksAsync(Arg.Any<KlineInterval>()).Returns(this.Candlesticks);

        // Act
        this.SUT.CFDMovingDown();
        await this.SUT.MakeMoveAsync();
        
        // Assert
        await this.FuturesTrader.Received(1).ClosePositionAsync();
        await this.Mediator.Received().Publish(Arg.Any<PositionClosedNotification>());
    }
    
    [Test]
    public async Task CFDMovingDown_ShouldNotTriggerPositionClosing_WhenTraderIsNotInPosition()
    {
        // Arrange
        this.FuturesTrader.IsInPosition().Returns(false);
        
        // Act
        this.SUT.CFDMovingDown();
        await this.SUT.MakeMoveAsync();

        // Assert
        await this.FuturesTrader.DidNotReceive().ClosePositionAsync();
    }
}