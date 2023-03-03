using Application.Exceptions;

using Binance.Net.Enums;

using Infrastructure.Services.Trading.Internal.Enums;

namespace Infrastructure.Tests.Integration.BinanceFuturesTradingServiceTestsBase.LongPositions;

public class OpenLongPositionTests : Base.BinanceFuturesTradingServiceTestsBase
{
    [Test]
    public async Task OpenPosition_ShouldOpenLongPosition_WhenInputIsCorrect()
    {
        // Arrange
        var current_price = await this.MarketDataProvider.GetCurrentPriceAsync(this.CurrencyPair.Name);


        // Act
        await this.SUT.PlaceMarketOrderAsync(OrderSide.Buy, this.Margin, 0.99m * current_price, 1.01m * current_price);
        

        // Assert
        this.SUT.IsInPosition().Should().BeTrue();
        this.SUT.Position!.CurrencyPair.Should().Be(this.CurrencyPair);
        this.SUT.Position!.Side.Should().Be(PositionSide.Long);
        this.SUT.Position!.Margin.Should().Be(this.Margin);
        this.SUT.Position!.StopLossOrder.Should().NotBeNull();
        this.SUT.Position!.TakeProfitOrder.Should().NotBeNull();

        this.SUT.Position.StopLossPrice.Should().BeApproximately(0.99m * this.SUT.Position.EntryPrice, precision);
        this.SUT.Position.TakeProfitPrice.Should().BeApproximately(1.01m * this.SUT.Position.EntryPrice, precision);
        
        var longPosition = await this.AccountDataProvider.GetPositionAsync(this.CurrencyPair.Name, PositionSide.Long);
        longPosition.Symbol.Should().Be(this.CurrencyPair.Name);
        longPosition.Should().NotBeNull();
        longPosition.PositionSide.Should().Be(PositionSide.Long);
        longPosition.EntryPrice.Should().BeApproximately(current_price, precision);
        longPosition.Quantity.Should().BeApproximately(this.Margin * this.Leverage / current_price, precision);
        
        this.SUT.OcoTaskStatus.Should().Be(OcoTaskStatus.Running);
        
        this.SUT.OcoIDs!.StopLoss.Should().Be(this.SUT.Position.StopLossOrder!.Id);
        this.SUT.OcoIDs!.TakeProfit.Should().Be(this.SUT.Position.TakeProfitOrder!.Id);
    }
    
    [Test]
    public async Task OpenPosition_ShouldNotOpenLongPosition_WhenInputIsIncorrect()
    {
        // Arrange
        var current_price = await this.MarketDataProvider.GetCurrentPriceAsync(this.CurrencyPair.Name);

        
        // Act
        var func = async () => await this.SUT.PlaceMarketOrderAsync(OrderSide.Buy, this.Margin, 1.01m * current_price, 0.99m * current_price);


        // Assert
        await func.Should().ThrowExactlyAsync<InvalidOrderException>();

        var longPosition = await this.AccountDataProvider.GetPositionAsync(this.CurrencyPair.Name, PositionSide.Long);
        longPosition.Should().BeNull();

        this.SUT.OcoTaskStatus.Should().Be(OcoTaskStatus.Unstarted);
        
        this.SUT.OcoIDs.Should().BeNull();
    }
}
