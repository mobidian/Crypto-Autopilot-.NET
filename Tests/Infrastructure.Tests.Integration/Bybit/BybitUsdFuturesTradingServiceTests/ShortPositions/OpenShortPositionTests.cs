using Application.Exceptions;

using Bybit.Net.Enums;

using FluentAssertions;

using Infrastructure.Tests.Integration.AbstractBases;
using Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.AbstractBase;

using Xunit;

namespace Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.ShortPositions;

public class OpenShortPositionTests : BybitUsdFuturesTradingServiceTestsBase
{
    public OpenShortPositionTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }


    [Theory]
    [InlineData(300, -300)] // Both StopLoss and TakeProfit specified
    [InlineData(300, null)] // Only StopLoss specified
    [InlineData(null, -300)] // Only TakeProfit specified
    [InlineData(null, null)] // Neither StopLoss nor TakeProfit specified
    public async Task OpenPosition_ShouldOpenShortPosition_WhenShortPositionDoesNotExist(int? stopLossOffset, int? takeProfitOffset)
    {
        // Arrange
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        decimal? stopLoss = stopLossOffset.HasValue ? lastPrice + stopLossOffset.Value : null;
        decimal? takeProfit = takeProfitOffset.HasValue ? lastPrice + takeProfitOffset.Value : null;
        var tradingStopTriggerType = TriggerType.LastPrice;
        
        // Act
        await this.SUT.OpenPositionAsync(PositionSide.Sell, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);
        
        // Assert
        this.SUT.ShortPosition.Should().NotBeNull();
        this.SUT.ShortPosition!.Side.Should().Be(PositionSide.Sell);
        this.SUT.ShortPosition!.Leverage.Should().Be(this.Leverage);
        this.SUT.ShortPosition!.StopLoss.Should().Be(stopLossOffset.HasValue ? stopLoss!.Value : 0);
        this.SUT.ShortPosition!.TakeProfit.Should().Be(takeProfitOffset.HasValue ? takeProfit!.Value : 0);
        
        var position = await this.FuturesAccount.GetPositionAsync(this.CurrencyPair.Name, PositionSide.Sell);
        position!.Side.Should().Be(PositionSide.Sell);
        position!.PositionMode.Should().Be(PositionMode.BothSideSell);
        position!.Leverage.Should().Be(this.Leverage);
        position!.StopLoss.Should().Be(stopLossOffset.HasValue ? stopLoss!.Value : 0);
        position!.TakeProfit.Should().Be(takeProfitOffset.HasValue ? takeProfit!.Value : 0);
        position!.StopLossTakeProfitMode.Should().Be(StopLossTakeProfitMode.Full);
    }

    [Fact]
    public async Task OpenPosition_ShouldThrow_WhenTradingStopParametersAreIncorrect()
    {
        // Arrange
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var stopLoss = lastPrice - 300;
        var takeProfit = lastPrice + 300;
        var tradingStopTriggerType = TriggerType.LastPrice;

        // Act
        var func = async () => await this.SUT.OpenPositionAsync(PositionSide.Sell, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);

        // Assert
        await func.Should().ThrowExactlyAsync<InternalTradingServiceException>();
    }
}
