using Application.Data.Mapping;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests;

public class AddFuturesOrderTests : FuturesTradesDBServiceTestsBase
{
    [Test]
    public async Task AddFuturesOrderWithoutPositionGuid_ShouldAddFuturesOrder_WhenOrderShouldNotPointToPosition()
    {
        // Arrange
        var order = this.FuturesOrderGenerator.Generate($"default, {LimitOrder}, {SideBuy}");
        
        // Act
        await this.SUT.AddFuturesOrderAsync(order);

        // Assert
        this.DbContext.FuturesOrders.Single().ToDomainObject().Should().BeEquivalentTo(order);
    }
        
    [Test]
    public async Task AddFuturesOrderWithoutPositionGuid_ShouldThrow_WhenOrderRequiresPosition()
    {
        // Arrange
        var order = this.FuturesOrderGenerator.Generate($"default, {MarketOrder}, {SideBuy}");
        
        // Act
        var func = async () => await this.SUT.AddFuturesOrderAsync(order);

        // Assert
        (await func.Should().ThrowExactlyAsync<FluentValidation.ValidationException>()).And
                .Errors.Should().ContainSingle(error => error.ErrorMessage == "The order can't be a market order or a filled limit order in order, otherwise it would have opened a position");
    }

    
    [Test]
    public async Task AddFuturesOrderWithPositionGuid_ShouldAddFuturesOrder_WhenOrderRequiresPosition()
    {
        // Arrange
        var order = this.FuturesOrderGenerator.Generate($"default, {MarketOrder}, {SideBuy}, {OrderPositionLong}");
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSideLong}");
        await this.DbContext.FuturesPositions.AddAsync(position.ToDbEntity());
        await this.DbContext.SaveChangesAsync();
        
        // Act
        await this.SUT.AddFuturesOrderAsync(order, position.CryptoAutopilotId);

        // Assert
        this.DbContext.FuturesOrders.Single().ToDomainObject().Should().BeEquivalentTo(order);
    }

    [Test]
    public async Task AddFuturesOrderWithPositionGuid_ShouldThrow_WhenOrderShouldNotPointToPosition()
    {
        // Arrange
        var order = this.FuturesOrderGenerator.Generate($"default, {LimitOrder}, {SideBuy}, {OrderPositionLong}");
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSideLong}");
        await this.DbContext.FuturesPositions.AddAsync(position.ToDbEntity());
        await this.DbContext.SaveChangesAsync();
        
        // Act
        var func = async () => await this.SUT.AddFuturesOrderAsync(order, position.CryptoAutopilotId);

        // Assert
        (await func.Should().ThrowExactlyAsync<FluentValidation.ValidationException>()).And
                .Errors.Should().ContainSingle(error => error.ErrorMessage == "The order must be a market order or a filled limit order in order, otherwise it cannot have opened a position");
    }


    [Test]
    public async Task AddFuturesOrderWithPositionGuid_ShouldThrow_WhenTheOrderPositionSideDoesNotMatchTheSideOfThePosition()
    {
        // Arrange
        var order = this.FuturesOrderGenerator.Generate($"default, {MarketOrder}, {SideBuy}, {OrderPositionLong}");
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSideShort}");
        await this.DbContext.FuturesPositions.AddAsync(position.ToDbEntity());
        await this.DbContext.SaveChangesAsync();

        // Act
        var func = async () => await this.SUT.AddFuturesOrderAsync(order, position.CryptoAutopilotId);
        
        // Assert
        (await func.Should().ThrowExactlyAsync<FluentValidation.ValidationException>()).And
                .Errors.Should().ContainSingle(error => error.ErrorMessage == "All orders position side must match the side of the position");
    }
}
