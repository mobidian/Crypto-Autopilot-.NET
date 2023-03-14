using Application.Data.Mapping;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests;

public class AddFuturesPositionTests : FuturesTradesDBServiceTestsBase
{
    [Test]
    public async Task AddFuturesPosition_ShouldAddFuturesPositionAndOrders_WhenAllFuturesOrdersRequirePosition()
    {
        // Arrange
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSideLong}");
        var orders = this.FuturesOrderGenerator.Generate(10, $"default, {MarketOrder}, {SideBuy}, {OrderPositionLong}");

        // Act
        await this.SUT.AddFuturesPositionAsync(position, orders);
        
        // Assert
        this.DbContext.FuturesPositions.Single().ToDomainObject().Should().BeEquivalentTo(position);
        this.DbContext.FuturesOrders.Select(x => x.ToDomainObject()).Should().BeEquivalentTo(orders);
    }
    
    [Test]
    public async Task AddFuturesPosition_ShouldThrow_WhenAnyFuturesOrderDoesNotRequirePosition()
    {
        // Arrange
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSideLong}");
        var orders = new[]
        {
            this.FuturesOrderGenerator.Generate($"default, {MarketOrder}, {SideBuy}"),
            this.FuturesOrderGenerator.Generate($"default, {LimitOrder}, {SideBuy}"), // does not need to point to position
        };

        // Act
        var func = async () => await this.SUT.AddFuturesPositionAsync(position, orders);

        // Assert
        (await func.Should().ThrowExactlyAsync<FluentValidation.ValidationException>()).And
                .Errors.Should().ContainSingle(error => error.ErrorMessage == "All orders must have opened a position");
    }
    
    [Test]
    public async Task AddFuturesPosition_ShouldThrow_WhenThePositionSideOfTheAnyOrderDoesNotMatchTheSideOfThePosition()
    {
        // Arrange
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSideLong}");
        var orders = this.FuturesOrderGenerator.Generate(10, $"default, {MarketOrder}, {SideBuy}, {OrderPositionShort}");
        
        // Act
        var func = async () => await this.SUT.AddFuturesPositionAsync(position, orders);
        
        // Assert
        (await func.Should().ThrowExactlyAsync<FluentValidation.ValidationException>()).And
                .Errors.Should().ContainSingle(error => error.ErrorMessage == "All orders position side must match the side of the position");
    }
}
