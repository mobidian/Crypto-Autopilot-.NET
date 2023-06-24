using Application.Exceptions;

using Bybit.Net.Enums;

using FluentAssertions;

using Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.AbstractBase;
using Infrastructure.Tests.Integration.Common.Fixtures;

using Xunit;

namespace Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.LongPositions;

public class ModifyTradingStopOnLong : BybitUsdFuturesTradingServiceTestsBase
{
    public ModifyTradingStopOnLong(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }


    [Theory]
    [InlineData(100, -100)] // Both StopLoss and TakeProfit specified
    [InlineData(100, 0)] // Only StopLoss specified
    [InlineData(0, -100)] // Only TakeProfit specified
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
        this.SUT.Positions.Single(x => x.Side == PositionSide.Buy).Should().BeEquivalentTo(this.SUT.LongPosition);
        this.SUT.LongPosition.Should().NotBeNull();
        this.SUT.LongPosition!.StopLoss.Should().Be(newStopLoss);
        this.SUT.LongPosition!.TakeProfit.Should().Be(newTakeProfit);
    }

    [Fact]
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
        await func.Should().ThrowExactlyAsync<InvalidOrderException>("No open Buy position was found");
    }
}
