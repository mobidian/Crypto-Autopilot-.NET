using Infrastructure.Tests.Integration.BinanceCfdTradingServiceTests.Base;

namespace Infrastructure.Tests.Integration.BinanceCfdTradingServiceTests.AllPositions;

public class ClosePositionTests : BinanceFuturesTradingServiceTestsBase
{
    [Test]
    public async Task ClosePosition_ShouldReturnNull_WhenPositionDoesNotExist()
    {
        // Act
        var func = this.SUT.ClosePositionAsync;
        
        // Assert
        (await func.Should().ThrowExactlyAsync<InvalidOperationException>()).WithMessage("No position is open")
            .WithInnerExceptionExactly<NullReferenceException>().WithMessage($"Position is NULL");
    }
}
