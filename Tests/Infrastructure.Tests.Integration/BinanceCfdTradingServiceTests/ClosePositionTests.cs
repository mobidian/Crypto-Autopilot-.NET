using Binance.Net.Enums;

using CryptoExchange.Net.Objects;

using FluentAssertions;

using Infrastructure.Tests.Integration.BinanceCfdTradingServiceTests.Common;

namespace Infrastructure.Tests.Integration.BinanceCfdTradingServiceTests;

public class ClosePositionTests : BinanceCfdTradingServiceTestsBase
{
    [Test, Order(1)]
    public async Task ClosePosition_ShouldCloseLongPosition_WhenLongPositionExists()
    {
        // Arrange
        decimal current_price = await this.SUT.GetCurrentPriceAsync();
        await this.SUT.OpenPositionAtMarketPriceAsync(OrderSide.Buy, this.testMargin, 0.99m * current_price, 1.01m * current_price);

        // Act
        await this.SUT.ClosePositionAsync();

        // Assert
        this.SUT.IsInPosition().Should().BeFalse();
    }

    [Test, Order(2)]
    public async Task ClosePosition_ShouldCloseShortPosition_WhenShortPositionExists()
    {
        // Arrange
        decimal current_price = await this.SUT.GetCurrentPriceAsync();
        await this.SUT.OpenPositionAtMarketPriceAsync(OrderSide.Sell, this.testMargin, 1.01m * current_price, 0.99m * current_price);

        // Act
        await this.SUT.ClosePositionAsync();

        // Assert
        this.SUT.IsInPosition().Should().BeFalse();
    }

    [Test, Order(3)]
    public async Task ClosePosition_ShouldReturnNull_WhenPositionDoesNotExist()
    {
        // Act
        var func = this.SUT.ClosePositionAsync;

        // Assert
        (await func.Should().ThrowExactlyAsync<InvalidOperationException>()).WithMessage("No position is open")
            .WithInnerExceptionExactly<NullReferenceException>()
            .WithMessage($"Position is NULL");
    }
}