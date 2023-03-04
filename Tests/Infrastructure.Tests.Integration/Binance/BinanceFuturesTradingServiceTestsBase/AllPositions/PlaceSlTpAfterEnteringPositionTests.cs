using Binance.Net.Enums;

using Infrastructure.Services.Trading.Binance.Internal.Enums;

namespace Infrastructure.Tests.Integration.Binance.BinanceFuturesTradingServiceTestsBase.AllPositions;

public class PlaceStopLossTakeProfitAfterEnteringPositionTests : Base.BinanceFuturesTradingServiceTestsBase
{
    [Test]
    public async Task PlacingStopLossAndTakeProfit_ShouldPlaceStopLossAndTakeProfitAndStartTheOcoTask_WhenInPosition()
    {
        // Arrange
        var orderSide = this.faker.PickRandom<OrderSide>();

        await this.SUT.PlaceMarketOrderAsync(orderSide, this.Margin);

        var current_price = await this.MarketDataProvider.GetCurrentPriceAsync(this.CurrencyPair.Name);
        var stopLossPrice = orderSide == OrderSide.Buy ? current_price - 10 : current_price + 10;
        var takeProfitPrice = orderSide == OrderSide.Buy ? current_price + 10 : current_price - 10;


        // Act
        var stopLossOrder = await this.SUT.PlaceStopLossAsync(stopLossPrice);
        var statusAfterStopLoss = this.SUT.OcoTaskStatus;

        var takeProfitOrder = await this.SUT.PlaceTakeProfitAsync(takeProfitPrice);
        var statusAfterTakeProfit = this.SUT.OcoTaskStatus;


        // Assert
        this.SUT.Position!.StopLossOrder!.Id.Should().Be(stopLossOrder.Id);
        this.SUT.Position!.StopLossPrice.Should().BeApproximately(stopLossOrder.StopPrice, precision);

        this.SUT.Position!.TakeProfitOrder!.Id.Should().Be(takeProfitOrder.Id);
        this.SUT.Position!.TakeProfitPrice.Should().BeApproximately(takeProfitOrder.StopPrice, precision);

        statusAfterStopLoss.Should().Be(OrderMonitoringTaskStatus.Unstarted);
        statusAfterTakeProfit.Should().Be(OrderMonitoringTaskStatus.Running);

        this.SUT.OcoIDs!.StopLoss.Should().Be(this.SUT.Position.StopLossOrder!.Id);
        this.SUT.OcoIDs!.TakeProfit.Should().Be(this.SUT.Position.TakeProfitOrder!.Id);
    }
}
