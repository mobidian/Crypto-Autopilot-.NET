using Infrastructure.Strategies.Example.Enums;
using Infrastructure.Tests.Unit.ExampleStrategyEngineTests.Base;

namespace Infrastructure.Tests.Unit.ExampleStrategyEngineTests;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[Parallelizable(ParallelScope.All)]
public class SignalConsumptionTests : ExampleStrategyEngineTestsBase
{
    [Test]
    public async Task OuterSignalBuy_ShouldBeConsumed_WhenItWasActedUpon()
    {
        // Arrange
        this.ArrangeFor_OuterSignalBuy_ShouldTriggerPositionOpening_WhenPriceIsAboveEmaAndTraderIsNotInPosition(out _, out _);

        // Act
        this.SUT.FlagDivergence(RsiDivergence.Bullish);
        await this.SUT.MakeMoveAsync();

        // Assert
        this.SUT.Divergence.Should().BeNull();
    }

    [Test]
    public void OuterSignalBuy_ShouldNotBeConsumed_WhenItWasNotActedUpon()
    {
        // Arrange
        this.ArrangeFor_OuterSignalBuy_ShouldNotTriggerPositionOpening_WhenPriceIsNotAboveEma(out _, out _);

        // Act
        this.SUT.FlagDivergence(RsiDivergence.Bullish);
        
        // simulates 100 candlesticks going by with no action beeing takes using the BUY alert
        var booleans = Enumerable.Range(0, 100).Select(i =>
        {
            this.SUT.MakeMoveAsync().GetAwaiter().GetResult();
            return this.SUT.Divergence == RsiDivergence.Bullish;
        });

        // Assert
        booleans.Should().AllBeEquivalentTo(true);
    }


    [Test]
    public async Task OuterSignalSell_ShouldBeConsumed_WhenItWasActedUpon()
    {
        // Arrange
        this.ArrangeFor_OuterSignalSell_ShouldTriggerPositionClosing_WhenTraderIsInPosition(out _);

        // Act
        this.SUT.FlagDivergence(RsiDivergence.Bearish);
        await this.SUT.MakeMoveAsync();

        // Assert
        this.SUT.Divergence.Should().BeNull();
    }

    [Test]
    public async Task OuterSignalSell_ShouldNotBeConsumed_WhenItWasNotActedUpon()
    {
        // Arrange
        this.ArrangeFor_OuterSignalSell_ShouldNotTriggerPositionClosing_WhenTraderIsInNotPosition();
        
        // Act
        this.SUT.FlagDivergence(RsiDivergence.Bearish);
        await this.SUT.MakeMoveAsync();

        // Assert
        this.SUT.Divergence.Should().Be(RsiDivergence.Bearish);
    }
}
