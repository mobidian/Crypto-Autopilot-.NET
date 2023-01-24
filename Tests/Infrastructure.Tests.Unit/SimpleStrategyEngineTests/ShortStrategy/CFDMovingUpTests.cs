using Binance.Net.Enums;

using Infrastructure.Notifications;

namespace Infrastructure.Tests.Unit.SimpleStrategyEngineTests.ShortStrategy;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class CFDMovingUpTests : ShortStrategyEngineTestsBase
{
    [Test]
    public async Task CFDMovingUp_ShouldTriggerPositionClosing_WhenTraderIsInPosition()
    {
        // Arrange
        decimal currentPrice = this.Random.Next(1000, 3000);
        this.FuturesTrader.GetCurrentPriceAsync().Returns(currentPrice);
        this.FuturesTrader.IsInPosition().Returns(true);
        this.FuturesDataProvider.GetCompletedCandlesticksAsync(Arg.Any<string>(), Arg.Any<KlineInterval>()).Returns(this.Candlesticks);
        
        // Act
        this.SUT.CFDMovingUp();
        await this.SUT.MakeMoveAsync();

        // Assert
        await this.FuturesTrader.Received(1).ClosePositionAsync();
        await this.Mediator.Received().Publish(Arg.Any<PositionClosedNotification>());
        this.SUT.Signal.Should().BeNull();
    }
    

    [Test]
    public async Task CFDMovingUp_ShouldNotTriggerPositionClosing_WhenTraderIsInNotPosition()
    {
        // Act
        this.SUT.CFDMovingUp();
        await this.SUT.MakeMoveAsync();

        // Assert
        await this.FuturesTrader.DidNotReceive().ClosePositionAsync();
        await this.Mediator.DidNotReceive().Publish(Arg.Any<PositionClosedNotification>());
        this.SUT.Signal.Should().BeNull();
    }
}
