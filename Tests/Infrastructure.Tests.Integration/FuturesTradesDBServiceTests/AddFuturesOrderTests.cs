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
        (await func.Should()
            .ThrowExactlyAsync<DbUpdateException>().WithMessage("An error occurred while saving the entity changes. See the inner exception for details."))
            .WithInnerExceptionExactly<ArgumentException>().WithMessage("A created market order or a filled limit order needs to be associated with a position and no position identifier has been specified.")
            .WithInnerExceptionExactly<FluentValidation.ValidationException>();
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
        (await func.Should()
            .ThrowExactlyAsync<DbUpdateException>().WithMessage("An error occurred while saving the entity changes. See the inner exception for details."))
            .WithInnerExceptionExactly<ArgumentException>().WithMessage("Only a created market order or a filled limit order can be associated with a position.")
            .WithInnerExceptionExactly<FluentValidation.ValidationException>(); 
    }


    [Test]
    public async Task AddFuturesOrderWithPositionGuid_ShouldThrow_WhenTheOrderPositionSideDoesNotMatchThePositionSide()
    {
        // Arrange
        var order = this.FuturesOrderGenerator.Generate($"default, {MarketOrder}, {SideBuy}, {OrderPositionLong}");
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSideShort}");
        await this.DbContext.FuturesPositions.AddAsync(position.ToDbEntity());
        await this.DbContext.SaveChangesAsync();

        // Act
        var func = async () => await this.SUT.AddFuturesOrderAsync(order, position.CryptoAutopilotId);
        
        // Assert
        (await func.Should()
            .ThrowExactlyAsync<DbUpdateException>().WithMessage("An error occurred while saving the entity changes. See the inner exception for details."))
            .WithInnerException<ArgumentException>().WithMessage($"The position side of the order did not match the side of the position with CryptoAutopilotId {position.CryptoAutopilotId}");
    }
}
