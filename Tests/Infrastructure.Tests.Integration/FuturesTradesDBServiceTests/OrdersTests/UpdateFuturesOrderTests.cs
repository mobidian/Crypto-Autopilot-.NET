﻿using Application.Data.Mapping;

using Bybit.Net.Enums;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;
using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Extensions;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.OrdersTests;

public class UpdateFuturesOrderTests : FuturesTradesDBServiceTestsBase
{
    [Test]
    public async Task UpdateFuturesOrderWithoutPositionGuid_ShouldUpdateFuturesOrder_WhenOrderShouldNotPointToPosition()
    {
        // Arrange
        var order = this.FuturesOrdersGenerator.Generate($"default, {OrderType.Limit.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}");
        await this.DbContext.FuturesOrders.AddAsync(order.ToDbEntity());
        await this.DbContext.SaveChangesAsync();

        var updatedOrder = this.FuturesOrdersGenerator.Clone().RuleFor(x => x.BybitID, order.BybitID).Generate($"default, {OrderType.Limit.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}");


        // Act
        await this.SUT.UpdateFuturesOrderAsync(updatedOrder.BybitID, updatedOrder);


        // Assert
        this.DbContext.FuturesOrders.Single().ToDomainObject().Should().BeEquivalentTo(updatedOrder);
    }

    [Test]
    public async Task UpdateFuturesOrderWithoutPositionGuid_ShouldThrow_WhenOrderRequiresPosition()
    {
        // Arrange
        var order = this.FuturesOrdersGenerator.Generate($"default, {OrderType.Limit.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}");
        await this.DbContext.FuturesOrders.AddAsync(order.ToDbEntity());
        await this.DbContext.SaveChangesAsync();

        var updatedOrder = this.FuturesOrdersGenerator.Clone()
            .RuleFor(x => x.BybitID, order.BybitID)
            .Generate($"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}");


        // Act
        var func = async () => await this.SUT.UpdateFuturesOrderAsync(updatedOrder.BybitID, updatedOrder);


        // Assert
        (await func.Should()
            .ThrowExactlyAsync<DbUpdateException>()
            .WithMessage("An error occurred while validating relationships between entities. The database update operation cannot be performed."))
                .WithInnerExceptionExactly<FluentValidation.ValidationException>()
                .And.Errors.Should().ContainSingle(error => error.ErrorMessage == "A market order must point to a position.");
    }


    [Test]
    public async Task UpdateFuturesOrderWithPositionGuid_ShouldUpdateFuturesOrder_WhenOrderRequiresPosition()
    {
        // Arrange
        var order = this.FuturesOrdersGenerator.Generate($"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}, {PositionSide.Buy.ToRuleSetName()}");
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSide.Buy.ToRuleSetName()}");
        await InsertRelatedPositionAndOrdersAsync(position, new[] { order });

        var updatedOrder = this.FuturesOrdersGenerator.Clone().RuleFor(x => x.BybitID, order.BybitID).Generate($"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}, {PositionSide.Buy.ToRuleSetName()}");


        // Act
        await this.SUT.UpdateFuturesOrderAsync(updatedOrder.BybitID, updatedOrder, position.CryptoAutopilotId);


        // Assert
        this.DbContext.FuturesOrders.Single().ToDomainObject().Should().BeEquivalentTo(updatedOrder);
    }

    [Test]
    public async Task UpdateFuturesOrderWithPositionGuid_ShouldThrow_WhenOrderShouldNotPointToPosition()
    {
        // Arrange
        var order = this.FuturesOrdersGenerator.Generate($"default, {OrderType.Limit.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}, {PositionSide.Buy.ToRuleSetName()}");
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSide.Buy.ToRuleSetName()}");

        await this.DbContext.FuturesOrders.AddAsync(order.ToDbEntity());
        await this.DbContext.FuturesPositions.AddAsync(position.ToDbEntity());
        await this.DbContext.SaveChangesAsync();

        var updatedOrder = this.FuturesOrdersGenerator.Clone().RuleFor(x => x.BybitID, order.BybitID).Generate($"default, {OrderType.Limit.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}");


        // Act
        var func = async () => await this.SUT.UpdateFuturesOrderAsync(updatedOrder.BybitID, updatedOrder, position.CryptoAutopilotId);


        // Assert
        (await func.Should()
            .ThrowExactlyAsync<DbUpdateException>()
            .WithMessage("An error occurred while validating relationships between entities. The database update operation cannot be performed."))
                .WithInnerExceptionExactly<FluentValidation.ValidationException>()
                .And.Errors.Should().ContainSingle(error => error.ErrorMessage == "A limit order which has not been filled must not point to a position.");
    }


    [Test]
    public async Task UpdateFuturesOrderWithPositionGuid_ShouldThrow_WhenTheOrderPositionSideDoesNotMatchThePositionSide()
    {
        // Arrange
        var order = this.FuturesOrdersGenerator.Generate($"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}, {PositionSide.Buy.ToRuleSetName()}");
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSide.Buy.ToRuleSetName()}");
        await InsertRelatedPositionAndOrdersAsync(position, new[] { order });

        var updatedOrder = this.FuturesOrdersGenerator.Clone().RuleFor(x => x.BybitID, order.BybitID).Generate($"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}, {PositionSide.Sell.ToRuleSetName()}");


        // Act
        var func = async () => await this.SUT.UpdateFuturesOrderAsync(updatedOrder.BybitID, updatedOrder, position.CryptoAutopilotId);


        // Assert
        (await func.Should()
            .ThrowExactlyAsync<DbUpdateException>()
            .WithMessage("An error occurred while validating relationships between entities. The database update operation cannot be performed."))
                .WithInnerExceptionExactly<FluentValidation.ValidationException>()
                .And.Errors.Should().ContainSingle(error => error.ErrorMessage == "An order which points to a position must have the appropriate value for the PositionSide property.");
    }

    
    [Test]
    public async Task UpdateFuturesOrderWithPositionGuid_ShouldThrow_WhenThePositionSideDoesNotMatchTheOldPositionSideAndItIsAlreadyPointingToSomePosition()
    {
        // Arrange
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSide.Buy.ToRuleSetName()}");
        var orders = this.FuturesOrdersGenerator.Generate(10, $"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}, {PositionSide.Buy.ToRuleSetName()}");
        await InsertRelatedPositionAndOrdersAsync(position, orders);

        var n = Random.Shared.Next(orders.Count);
        var updatedOrder = this.FuturesOrdersGenerator.Clone()
            .RuleFor(x => x.BybitID, orders[n].BybitID)
            .Generate($"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}, {PositionSide.Sell.ToRuleSetName()}");


        // Act
        var func = async () => await this.SUT.UpdateFuturesOrderAsync(updatedOrder.BybitID, updatedOrder);

        
        // Assert
        (await func.Should()
            .ThrowExactlyAsync<DbUpdateException>()
            .WithMessage("An error occurred while validating relationships between entities. The database update operation cannot be performed."))
                .WithInnerExceptionExactly<FluentValidation.ValidationException>()
                .And.Errors.Should().ContainSingle(error => error.ErrorMessage == "An order which points to a position must have the appropriate value for the PositionSide property.");
    }
}
