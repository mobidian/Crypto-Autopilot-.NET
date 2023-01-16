using Application.Exceptions;

using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;

using FluentAssertions;

using Infrastructure.Tests.Integration.BinanceCfdTradingServiceTests.Common;

namespace Infrastructure.Tests.Integration.BinanceCfdTradingServiceTests;

public class OpenPositionTests : BinanceCfdTradingServiceTestsBase
{
    private const decimal precision = 1; // for assertions
    

    [Test, Order(1)]
    public async Task OpenPosition_ShouldOpenLongPosition_WhenInputIsCorrect()
    {
        // Arrange
        decimal current_price = await this.SUT.GetCurrentPriceAsync();

        // Act
        var CallResult = await this.SUT.OpenPositionAtMarketPriceAsync(OrderSide.Buy, this.testMargin, 0.99m * current_price, 1.01m * current_price);

        // Assert
        this.SUT.IsInPosition().Should().BeTrue();
        this.SUT.Position!.Margin.Should().Be(this.testMargin);
        this.SUT.Position!.StopLossOrder.Should().NotBeNull();
        this.SUT.Position!.TakeProfitOrder.Should().NotBeNull();

        this.SUT.Position.StopLossPrice.Should().BeApproximately(0.99m * this.SUT.Position.EntryPrice, precision);
        this.SUT.Position.TakeProfitPrice.Should().BeApproximately(1.01m * this.SUT.Position.EntryPrice, precision);
    }

    [Test, Order(2)]
    public async Task OpenPosition_ShouldNotOpenLongPosition_WhenInputIsIncorrect()
    {
        // Arrange
        decimal current_price = await this.SUT.GetCurrentPriceAsync();

        // Act
        var func = async () => await this.SUT.OpenPositionAtMarketPriceAsync(OrderSide.Buy, this.testMargin, 1.01m * current_price, 0.99m * current_price);

        // Assert
        await func.Should().ThrowExactlyAsync<InvalidOrderException>();
    }


    [Test, Order(3)]
    public async Task OpenPosition_ShouldOpenShortPosition_WhenInputIsCorrect()
    {
        // Arrange
        decimal current_price = await this.SUT.GetCurrentPriceAsync();

        // Act
        var CallResult = await this.SUT.OpenPositionAtMarketPriceAsync(OrderSide.Sell, this.testMargin, 1.01m * current_price, 0.99m * current_price);

        // Assert
        this.SUT.IsInPosition().Should().BeTrue();
        this.SUT.Position!.Margin.Should().Be(this.testMargin);
        this.SUT.Position!.StopLossOrder.Should().NotBeNull();
        this.SUT.Position!.TakeProfitOrder.Should().NotBeNull();

        this.SUT.Position.StopLossPrice.Should().BeApproximately(1.01m * this.SUT.Position.EntryPrice, precision);
        this.SUT.Position.TakeProfitPrice.Should().BeApproximately(0.99m * this.SUT.Position.EntryPrice, precision);
    }
    
    [Test, Order(4)]
    public async Task OpenPosition_ShouldNotOpenShortPosition_WhenInputIsIncorrect()
    {
        // Arrange
        decimal current_price = await this.SUT.GetCurrentPriceAsync();

        // Act
        var func = async () => await this.SUT.OpenPositionAtMarketPriceAsync(OrderSide.Sell, this.testMargin, 0.99m * current_price, 1.01m * current_price);

        // Assert
        await func.Should().ThrowExactlyAsync<InvalidOrderException>();
    }

    
    [Test, Order(5)]
    public async Task OpenPosition_ShouldThrow_WhenPositionIsAlreadyOpen()
    {
        // Arrange
        await this.SUT.OpenPositionAtMarketPriceAsync(OrderSide.Buy, this.testMargin);

        // Act
        var func = async () => await this.SUT.OpenPositionAtMarketPriceAsync(OrderSide.Buy, this.testMargin);

        // Assert
        await func.Should().ThrowExactlyAsync<InvalidOperationException>().WithMessage("A position is open already");
    }
}
