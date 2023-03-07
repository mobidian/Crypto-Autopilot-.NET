using Bybit.Net.Enums;

using Infrastructure.Extensions.Bybit;
using Infrastructure.Tests.Integration.Bybit.BybitFuturesAccountDataProviderTests.AbstractBase;

namespace Infrastructure.Tests.Integration.Bybit.BybitFuturesAccountDataProviderTests;

public class GetPositionTests : BybitFuturesAccountDataProviderTestsBase
{
    [Test]
    [TestCase(OrderSide.Buy)]
    [TestCase(OrderSide.Sell)]
    public async Task GetPositionAsync_ShouldReturnPosition_WhenPositionExists(OrderSide entryOrderSide)
    {
        // Arrange
        var quantity = 0.005m;

        var currentPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var stopLoss = entryOrderSide == OrderSide.Buy ? currentPrice - 100 : currentPrice + 100;
        var takeProfit = entryOrderSide == OrderSide.Buy ? currentPrice + 100 : currentPrice - 100;

        var positionSide = entryOrderSide.ToPositionSide();
        var positionMode = entryOrderSide.ToPositionMode();

        await this.TradingClient_PlaceOrderAsync(this.CurrencyPair.Name, entryOrderSide, OrderType.Market, quantity, TimeInForce.ImmediateOrCancel, false, false, stopLossPrice: stopLoss, stopLossTriggerType: TriggerType.MarkPrice, takeProfitPrice: takeProfit, takeProfitTriggerType: TriggerType.MarkPrice, positionMode: positionMode);


        // Act
        var position = await this.SUT.GetPositionAsync(this.CurrencyPair.Name, positionSide);


        // Assert
        position.Should().NotBeNull();
        position!.Side.Should().Be(positionSide);
        position!.PositionMode.Should().Be(positionMode);
        position!.Quantity.Should().Be(quantity);
        position!.StopLoss.Should().Be(stopLoss);
        position!.TakeProfit.Should().Be(takeProfit);
    }

    [Test]
    public async Task GetPositionAsync_ShouldReturnNull_WhenPositionDoesNotExist()
    {
        // Act
        var position = await this.SUT.GetPositionAsync(this.CurrencyPair.Name, PositionSide.Buy);

        // Assert
        position.Should().BeNull();
    }
}
