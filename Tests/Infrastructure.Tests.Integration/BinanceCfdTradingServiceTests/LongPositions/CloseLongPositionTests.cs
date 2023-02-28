using Binance.Net.Enums;

using Infrastructure.Tests.Integration.BinanceCfdTradingServiceTests.Base;

namespace Infrastructure.Tests.Integration.BinanceCfdTradingServiceTests.LongPositions;

public class CloseLongPositionTests : BinanceCfdTradingServiceTestsBase
{
    [Test]
    public async Task ClosePosition_ShouldCloseLongPosition_WhenLongPositionExists()
    {
        // Arrange
        var current_price = await this.CfdMarketDataProvider.GetCurrentPriceAsync(this.CurrencyPair.Name);
        await this.SUT.PlaceMarketOrderAsync(OrderSide.Buy, this.testMargin, 0.99m * current_price, 1.01m * current_price);

        // Act
        await this.SUT.ClosePositionAsync();

        // Assert
        this.SUT.IsInPosition().Should().BeFalse();
    }
}