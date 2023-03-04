using Application.Exceptions;

using Infrastructure.Tests.Integration.BinanceFuturesTradingServiceTestsBase.Base;

namespace Infrastructure.Tests.Integration.BinanceFuturesTradingServiceTestsBase.AllPositions;

public class ClosePositionTests : Base.BinanceFuturesTradingServiceTestsBase
{
    [Test]
    public async Task ClosePosition_ShouldThrow_WhenPositionDoesNotExist()
    {
        // Act
        var func = this.SUT.ClosePositionAsync;
        
        // Assert
        (await func.Should().ThrowExactlyAsync<InvalidOrderException>()).WithMessage("No position is open")
            .WithInnerExceptionExactly<NullReferenceException>().WithMessage($"Position is NULL");
    }
}
