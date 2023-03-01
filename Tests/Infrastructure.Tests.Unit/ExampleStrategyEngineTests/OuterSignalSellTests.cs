using Infrastructure.Notifications;
using Infrastructure.Strategies.Example.Enums;
using Infrastructure.Tests.Unit.ExampleStrategyEngineTests.Base;

namespace Infrastructure.Tests.Unit.ExampleStrategyEngineTests;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[Parallelizable(ParallelScope.All)]
public class OuterSignalSellTests : ExampleStrategyEngineTestsBase
{
    [Test]
    public async Task OuterSignalSell_ShouldTriggerPositionClosing_WhenTraderIsInPosition()
    {
        // Arrange
        this.ArrangeFor_OuterSignalSell_ShouldTriggerPositionClosing_WhenTraderIsInPosition(out _);

        // Act
        this.SUT.FlagDivergence(RsiDivergence.Bearish);
        await this.SUT.MakeMoveAsync();

        // Assert
        await this.FuturesTrader.Received(1).ClosePositionAsync();
        await this.Mediator.Received(1).Publish(Arg.Any<PositionClosedNotification>());
    }
    
    [Test]
    public async Task OuterSignalSell_ShouldNotTriggerPositionClosing_WhenTraderIsInNotPosition()
    {
        // Arrange
        this.ArrangeFor_OuterSignalSell_ShouldNotTriggerPositionClosing_WhenTraderIsInNotPosition();

        // Act
        this.SUT.FlagDivergence(RsiDivergence.Bearish);
        await this.SUT.MakeMoveAsync();

        // Assert
        await this.FuturesTrader.DidNotReceive().ClosePositionAsync();
        await this.Mediator.DidNotReceive().Publish(Arg.Any<PositionClosedNotification>());
    }
}
