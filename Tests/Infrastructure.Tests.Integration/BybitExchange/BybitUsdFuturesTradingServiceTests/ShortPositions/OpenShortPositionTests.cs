using Application.Exceptions;

using Bybit.Net.Enums;

using Infrastructure.Tests.Integration.BybitExchange.BybitUsdFuturesTradingServiceTests.AbstractBase;

namespace Infrastructure.Tests.Integration.BybitExchange.BybitUsdFuturesTradingServiceTests.ShortPositions;

public class OpenShortPositionTests : BybitUsdFuturesTradingServiceTestsBase
{
    [Test]
    public async Task OpenPosition_OpensLongPosition_WhenShortPositionDoesNotExist()
    {
        // Arrange
        var lastPrice = await this.FuturesDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var stopLoss = lastPrice + 300;
        var takeProfit = lastPrice - 300;
        var tradingStopTriggerType = TriggerType.LastPrice;

        // Act
        await this.SUT.OpenPositionAsync(PositionSide.Sell, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);
        
        // Assert
        this.SUT.ShortPosition.Should().NotBeNull();
        this.SUT.ShortPosition!.Side.Should().Be(PositionSide.Sell);
        this.SUT.ShortPosition!.PositionMode.Should().Be(PositionMode.BothSideSell);
        this.SUT.ShortPosition!.Leverage.Should().Be(this.Leverage);
        this.SUT.ShortPosition!.StopLoss.Should().Be(stopLoss);
        this.SUT.ShortPosition!.TakeProfit.Should().Be(takeProfit);
    }

    [Test]
    public async Task OpenPosition_ShouldThrow_WhenTradingStopParametersAreIncorrect()
    {
        // Arrange
        var lastPrice = await this.FuturesDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var stopLoss = lastPrice - 300;
        var takeProfit = lastPrice + 300;
        var tradingStopTriggerType = TriggerType.LastPrice;

        // Act
        var func = async () => await this.SUT.OpenPositionAsync(PositionSide.Sell, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);

        // Assert
        await func.Should().ThrowExactlyAsync<InternalTradingServiceException>();
    }

    [Test]
    public async Task OpenPosition_ShouldThrow_WhenShortPositionIsOpenAlready()
    {
        // Arrange
        var lastPrice = await this.FuturesDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var stopLoss = lastPrice + 300;
        var takeProfit = lastPrice - 300;
        var tradingStopTriggerType = TriggerType.LastPrice;

        await this.SUT.OpenPositionAsync(PositionSide.Sell, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);

        // Act
        var func = async () => await this.SUT.OpenPositionAsync(PositionSide.Sell, this.Margin, stopLoss, takeProfit, tradingStopTriggerType);

        // Assert
        await func.Should().ThrowExactlyAsync<InvalidOrderException>().WithMessage($"There is a Sell position open already");
    }
}
