using Binance.Net.Enums;

using Infrastructure.Tests.Integration.BinanceCfdTradingServiceTests.Base;

namespace Infrastructure.Tests.Integration.BinanceCfdTradingServiceTests.ShortPositions;

public class CloseShortPositionTests : BinanceCfdTradingServiceTestsBase
{
    [Test]
    public async Task ClosePosition_ShouldCloseShortPosition_WhenShortPositionExists()
    {
        // Arrange
        var current_price = await this.MarketDataProvider.GetCurrentPriceAsync(this.CurrencyPair.Name);
        await this.SUT.PlaceMarketOrderAsync(OrderSide.Sell, this.testMargin, 1.01m * current_price, 0.99m * current_price);

        // Act
        await this.SUT.ClosePositionAsync();

        // Assert
        this.SUT.IsInPosition().Should().BeFalse();
    }
}
