using Application.Exceptions;

using Bybit.Net.Enums;

using FluentAssertions;

using Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.AbstractBase;

using Tests.Integration.Common.Fixtures;

using Xunit;

namespace Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.CombinedTests;

public class ClosePositionTests : BybitUsdFuturesTradingServiceTestsBase
{
    public ClosePositionTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }


    [Theory]
    [InlineData(PositionSide.Buy, -300, 300)]
    [InlineData(PositionSide.Sell, 300, -300)]
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
        this.SUT.Positions.Should().BeEmpty();
        var position = positionSide == PositionSide.Buy ? this.SUT.LongPosition : this.SUT.ShortPosition;
        position.Should().BeNull();
    }

    [Theory]
    [InlineData(PositionSide.Buy, "No open Buy position was found")]
    [InlineData(PositionSide.Sell, "No open Sell position was found")]
    public async Task ClosePosition_ShouldThrow_WhenPositionDoesNotExist(PositionSide positionSide, string expectedErrorMessage)
    {
        // Act
        var func = async () => await this.SUT.ClosePositionAsync(positionSide);

        // Assert
        await func.Should().ThrowExactlyAsync<InvalidOrderException>(expectedErrorMessage);
    }
    
    [Fact]
    public async Task CloseAllPositionsAsync_ShouldCloseAllOpenPositions()
    {
        // Arrange
        await this.SUT.OpenPositionAsync(PositionSide.Buy, this.Margin);
        await this.SUT.OpenPositionAsync(PositionSide.Sell, this.Margin);

        // Act
        await this.SUT.CloseAllPositionsAsync();

        // Assert
        this.SUT.Positions.Should().BeEmpty();
        this.SUT.LongPosition.Should().BeNull();
        this.SUT.ShortPosition.Should().BeNull();
    }
}
