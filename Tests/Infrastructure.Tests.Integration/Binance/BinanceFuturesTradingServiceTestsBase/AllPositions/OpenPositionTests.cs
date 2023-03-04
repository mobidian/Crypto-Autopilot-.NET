using Application.Exceptions;

using Binance.Net.Enums;

namespace Infrastructure.Tests.Integration.Binance.BinanceFuturesTradingServiceTestsBase.AllPositions;

public class OpenPositionTests : Base.BinanceFuturesTradingServiceTestsBase
{
    [Test]
    public async Task OpenPosition_ShouldThrow_WhenPositionIsAlreadyOpen()
    {
        // Arrange
        await this.SUT.PlaceMarketOrderAsync((OrderSide)Random.Shared.Next(2), this.Margin);

        // Act
        var func = async () => await this.SUT.PlaceMarketOrderAsync(OrderSide.Buy, this.Margin);

        // Assert
        await func.Should().ThrowExactlyAsync<InvalidOrderException>().WithMessage("A position is open already");
    }
}
