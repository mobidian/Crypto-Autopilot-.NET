using System.Text;

using Application.Data.Mapping;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;

using Microsoft.EntityFrameworkCore;

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
        (await func.Should()
            .ThrowExactlyAsync<DbUpdateException>().WithMessage("An error occurred while saving the entity changes. See the inner exception for details."))
            .WithInnerExceptionExactly<FluentValidation.ValidationException>().And
                .Errors.Should().ContainSingle(error => error.ErrorMessage == "All orders must have opened a position");
    }


    [Test]
    public async Task AddFuturesPosition_ShouldThrow_WhenNotAllOrdersRequireTheSamePositionSide()
    {
        // Arrange
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSideLong}");
        var orders = new[]
        {
            this.FuturesOrderGenerator.Generate($"default, {MarketOrder}, {SideBuy}, {OrderPositionLong}"),
            this.FuturesOrderGenerator.Generate($"default, {MarketOrder}, {SideBuy}, {OrderPositionShort}"),
        };
        
        // Act
        var func = async () => await this.SUT.AddFuturesPositionAsync(position, orders);

        // Assert
        (await func.Should()
            .ThrowExactlyAsync<DbUpdateException>().WithMessage("An error occurred while saving the entity changes. See the inner exception for details."))
            .WithInnerExceptionExactly<FluentValidation.ValidationException>().And
                .Errors.Should().ContainSingle(error => error.ErrorMessage == "All orders must have the same position side");
    }
    
    [Test]
    public async Task AddFuturesPosition_ShouldThrow_WhenThePositionSideOfTheOrdersDoesNotMatchThePositionSide()
    {
        // Arrange
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSideLong}");
        var orders = this.FuturesOrderGenerator.Generate(10, $"default, {MarketOrder}, {SideBuy}, {OrderPositionShort}");
        
        // Act
        var func = async () => await this.SUT.AddFuturesPositionAsync(position, orders);
        
        // Assert
        (await func.Should()
            .ThrowExactlyAsync<DbUpdateException>().WithMessage("An error occurred while saving the entity changes. See the inner exception for details."))
            .WithInnerException<ArgumentException>().WithMessage($"The position side of the specified orders does not match the side of the specified position");
    }
}
