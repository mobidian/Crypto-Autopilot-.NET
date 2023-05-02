using Application.Data.Mapping;

using Bybit.Net.Enums;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;
using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Extensions;

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
        (await func.Should().ThrowExactlyAsync<FluentValidation.ValidationException>()).And
                .Errors.Should().ContainSingle(error => error.ErrorMessage == "The order can't be a market order or a filled limit order in order, otherwise it would have opened a position");
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
        (await func.Should().ThrowExactlyAsync<FluentValidation.ValidationException>()).And
                .Errors.Should().ContainSingle(error => error.ErrorMessage == "The order must be a market order or a filled limit order in order, otherwise it cannot have opened a position");
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
        (await func.Should().ThrowExactlyAsync<FluentValidation.ValidationException>()).And
                .Errors.Should().ContainSingle(error => error.ErrorMessage == "All orders position side must match the side of the position");
    }
}
