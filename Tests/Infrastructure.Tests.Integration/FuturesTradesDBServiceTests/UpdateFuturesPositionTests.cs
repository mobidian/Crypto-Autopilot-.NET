using Application.Data.Mapping;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests;

public class UpdateFuturesPositionTests : FuturesTradesDBServiceTestsBase
{
    [Test]
    public async Task UpdateFuturesPosition_ShouldUpdateFuturesPosition_WhenFuturesPositionIsValid()
    {
        // Arrange
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSideLong}");
        var orders = this.FuturesOrderGenerator.Generate(10, $"default, {MarketOrder}, {SideBuy}, {OrderPositionLong}");
        await InsertRelatedPositionAndOrdersAsync(position, orders);
        
        var updatedPosition = this.FuturesPositionsGenerator.Clone()
            .RuleFor(x => x.CryptoAutopilotId, position.CryptoAutopilotId)
            .Generate($"default, {PositionSideLong}");

        
        // Act
        await this.SUT.UpdateFuturesPositionAsync(updatedPosition.CryptoAutopilotId, updatedPosition);
        
        // Assert
        this.DbContext.FuturesPositions.Single().ToDomainObject().Should().BeEquivalentTo(updatedPosition);
    }
    
    [Test]
    public async Task UpdateFuturesPosition_ShouldThrow_WhenFuturesPositionSideDoesNotMatchTheOldFuturesPositionSide()
    {
        // Arrange
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSideLong}");
        var orders = this.FuturesOrderGenerator.Generate(10, $"default, {MarketOrder}, {SideBuy}, {OrderPositionLong}");
        await InsertRelatedPositionAndOrdersAsync(position, orders);

        var updatedPosition = this.FuturesPositionsGenerator.Clone()
            .RuleFor(x => x.CryptoAutopilotId, position.CryptoAutopilotId)
            .Generate($"default, {PositionSideShort}");


        // Act
        var func = async () => await this.SUT.UpdateFuturesPositionAsync(updatedPosition.CryptoAutopilotId, updatedPosition);

        // Assert
        (await func.Should().ThrowExactlyAsync<FluentValidation.ValidationException>()).And
                .Errors.Should().ContainSingle(error => error.ErrorMessage == "All orders position side must match the side of the position");
    }
}
