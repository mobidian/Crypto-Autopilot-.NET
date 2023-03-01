using Binance.Net.Enums;

using Infrastructure.Tests.Integration.BinanceCfdTradingServiceTests.Base;

namespace Infrastructure.Tests.Integration.BinanceCfdTradingServiceTests.AllPositions;

public class OpenPositionTests : BinanceFuturesTradingServiceTestsBase
{
    [Test]
    public async Task OpenPosition_ShouldThrow_WhenPositionIsAlreadyOpen()
    {
        // Arrange
        await this.SUT.PlaceMarketOrderAsync((OrderSide)Random.Shared.Next(2), this.testMargin);
        
        // Act
        var func = async () => await this.SUT.PlaceMarketOrderAsync(OrderSide.Buy, this.testMargin);

        // Assert
        await func.Should().ThrowExactlyAsync<InvalidOperationException>().WithMessage("A position is open already");
    }
}
