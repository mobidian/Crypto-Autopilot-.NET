using Binance.Net.Enums;

using Infrastructure.Strategies.SimpleStrategy.Notifications;
using Infrastructure.Tests.Unit.SimpleStrategyEngineTests.Base;

namespace Infrastructure.Tests.Unit.SimpleStrategyEngineTests;

public class CFDMovingUpTests : SimpleStrategyEngineTestsBase
{
    [Test]
    public async Task CFDMovingUp_ShouldTriggerPositionOpening_WhenTraderIsNotInPosition()
    {
        // Arrange
        decimal currentPrice = 1000;
        this.FuturesTrader.GetCurrentPriceAsync().Returns(currentPrice);
        this.FuturesTrader.IsInPosition().Returns(false, true);
        this.FuturesDataProvider.GetCompletedCandlesticksAsync(Arg.Any<KlineInterval>()).Returns(this.Candlesticks);

        // Act
        this.SUT.CFDMovingUp();
        await this.SUT.MakeMoveAsync();

        // Assert
        await this.FuturesTrader.Received(1).OpenPositionAtMarketPriceAsync(OrderSide.Buy, Arg.Any<decimal>(), 0.99m * currentPrice, 1.01m * currentPrice);
        await this.Mediator.Received().Publish(Arg.Any<PositionOpenedNotification>());
    }
    
    [Test]
    public async Task CFDMovingUp_ShouldNotTriggerPositionOpening_WhenTraderIsInPosition()
    {
        // Arrange
        this.FuturesTrader.IsInPosition().Returns(true);
        
        // Act
        this.SUT.CFDMovingUp();
        await this.SUT.MakeMoveAsync();

        // Assert
        await this.FuturesTrader.DidNotReceive().OpenPositionAtMarketPriceAsync(Arg.Any<OrderSide>(), Arg.Any<decimal>(), Arg.Any<decimal>(), Arg.Any<decimal>());
    }
}
