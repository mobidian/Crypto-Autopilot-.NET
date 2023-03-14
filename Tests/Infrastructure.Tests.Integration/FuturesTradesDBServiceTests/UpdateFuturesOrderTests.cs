using Application.Data.Mapping;

using Domain.Models;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests;

public class UpdateFuturesOrderTests : FuturesTradesDBServiceTestsBase
{
    [Test]
    public async Task UpdateFuturesOrderWithoutPositionGuid_ShouldUpdateFuturesOrder_WhenOrderShouldNotPointToPosition()
    {
        // Arrange
        var order = this.FuturesOrderGenerator.Generate($"default, {LimitOrder}, {SideBuy}");
        await this.DbContext.FuturesOrders.AddAsync(order.ToDbEntity());
        await this.DbContext.SaveChangesAsync();
        
        var updatedOrder = this.FuturesOrderGenerator.Clone().RuleFor(x => x.BybitID, order.BybitID).Generate($"default, {LimitOrder}, {SideBuy}");


        // Act
        await this.SUT.UpdateFuturesOrderAsync(updatedOrder.BybitID, updatedOrder);


        // Assert
        this.DbContext.FuturesOrders.Single().ToDomainObject().Should().BeEquivalentTo(updatedOrder);
    }

    [Test]
    public async Task UpdateFuturesOrderWithoutPositionGuid_ShouldThrow_WhenOrderRequiresPosition()
    {
        // Arrange
        var order = this.FuturesOrderGenerator.Generate($"default, {LimitOrder}, {SideBuy}");
        await this.DbContext.FuturesOrders.AddAsync(order.ToDbEntity());
        await this.DbContext.SaveChangesAsync();

        var updatedOrder = this.FuturesOrderGenerator.Clone()
            .RuleFor(x => x.BybitID, order.BybitID)
            .Generate($"default, {MarketOrder}, {SideBuy}");

        
        // Act
        var func = async () => await this.SUT.UpdateFuturesOrderAsync(updatedOrder.BybitID, updatedOrder);


        // Assert
        (await func.Should().ThrowExactlyAsync<FluentValidation.ValidationException>()).And
                .Errors.Should().ContainSingle(error => error.ErrorMessage == "The order can't be a market order or a filled limit order in order, otherwise it would have opened a position");
    }


    [Test]
    public async Task UpdateFuturesOrderWithPositionGuid_ShouldUpdateFuturesOrder_WhenOrderRequiresPosition()
    {
        // Arrange
        var order = this.FuturesOrderGenerator.Generate($"default, {MarketOrder}, {SideBuy}, {OrderPositionLong}");
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSideLong}");
        await InsertRelatedPositionAndOrderAsync(position, order);

        var updatedOrder = this.FuturesOrderGenerator.Clone().RuleFor(x => x.BybitID, order.BybitID).Generate($"default, {MarketOrder}, {SideBuy}, {OrderPositionLong}");

        
        // Act
        await this.SUT.UpdateFuturesOrderAsync(updatedOrder.BybitID, updatedOrder, position.CryptoAutopilotId);


        // Assert
        this.DbContext.FuturesOrders.Single().ToDomainObject().Should().BeEquivalentTo(updatedOrder);
    }
    
    [Test]
    public async Task UpdateFuturesOrderWithPositionGuid_ShouldThrow_WhenOrderShouldNotPointToPosition()
    {
        // Arrange
        var order = this.FuturesOrderGenerator.Generate($"default, {LimitOrder}, {SideBuy}, {OrderPositionLong}");
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSideLong}");

        await this.DbContext.FuturesOrders.AddAsync(order.ToDbEntity());
        await this.DbContext.FuturesPositions.AddAsync(position.ToDbEntity());
        await this.DbContext.SaveChangesAsync();
        
        var updatedOrder = this.FuturesOrderGenerator.Clone().RuleFor(x => x.BybitID, order.BybitID).Generate($"default, {LimitOrder}, {SideBuy}");


        // Act
        var func = async () => await this.SUT.UpdateFuturesOrderAsync(updatedOrder.BybitID, updatedOrder, position.CryptoAutopilotId);

        
        // Assert
        (await func.Should().ThrowExactlyAsync<FluentValidation.ValidationException>()).And
                .Errors.Should().ContainSingle(error => error.ErrorMessage == "The order must be a market order or a filled limit order in order, otherwise it cannot have opened a position");
    }


    [Test]
    public async Task UpdateFuturesOrderWithPositionGuid_ShouldThrow_WhenTheOrderPositionSideDoesNotMatchThePositionSide()
    {
        // Arrange
        var order = this.FuturesOrderGenerator.Generate($"default, {MarketOrder}, {SideBuy}, {OrderPositionLong}");
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSideLong}");
        await InsertRelatedPositionAndOrderAsync(position, order);

        var updatedOrder = this.FuturesOrderGenerator.Clone().RuleFor(x => x.BybitID, order.BybitID).Generate($"default, {MarketOrder}, {SideBuy}, {OrderPositionShort}");
        

        // Act
        var func = async () => await this.SUT.UpdateFuturesOrderAsync(updatedOrder.BybitID, updatedOrder, position.CryptoAutopilotId);

        
        // Assert
        (await func.Should().ThrowExactlyAsync<FluentValidation.ValidationException>()).And
                .Errors.Should().ContainSingle(error => error.ErrorMessage == "All orders position side must match the side of the position");
    }


    
    private async Task InsertRelatedPositionAndOrderAsync(FuturesPosition position, FuturesOrder order)
    {
        using var transaction = await this.DbContext.Database.BeginTransactionAsync();


        var orderEntity = order.ToDbEntity();
        orderEntity.Position = position.ToDbEntity();

        await this.DbContext.FuturesOrders.AddAsync(orderEntity);
        await this.DbContext.SaveChangesAsync();


        await transaction.CommitAsync();
    }
}
