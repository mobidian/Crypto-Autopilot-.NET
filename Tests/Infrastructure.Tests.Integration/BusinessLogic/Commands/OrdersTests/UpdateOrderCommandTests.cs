using Application.Data.Mapping;

using Domain.Commands.Orders;
using Domain.Commands.Positions;
using Domain.Models.Futures;

using FluentAssertions;

using Infrastructure.Tests.Integration.BusinessLogic.Commands.AbstractBase;
using Infrastructure.Tests.Integration.BusinessLogic.Commands.Common;
using Infrastructure.Tests.Integration.Common.Fixtures;
using Infrastructure.Tests.Integration.DataAccess.Extensions;

using Xunit;

namespace Infrastructure.Tests.Integration.BusinessLogic.Commands.OrdersTests;

public class UpdateOrderCommandTests : CommandsTestsBase
{
    public UpdateOrderCommandTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }


    [Theory]
    [MemberData(nameof(LimitOrdersGenerator.GetRuleSetsAsObjectArrays), MemberType = typeof(LimitOrdersGenerator))]
    public async Task UpdateOrderCommand_ShouldUpdateOrderToLimitOrder_WhenNoPositionIdIsSpecified(string ruleSet)
    {
        // Arrange
        var limitOrder = this.FuturesOrdersGenerator.Generate($"default, {ruleSet}");
        await this.Mediator.Send(new CreateLimitOrderCommand
        {
            LimitOrder = limitOrder
        });
        
        var updatedLimitOrder = this.FuturesOrdersGenerator.RuleFor(x => x.BybitID, limitOrder.BybitID).Generate($"default, {ruleSet}");
        var command = new UpdateOrderCommand
        {
            UpdatedOrder = updatedLimitOrder
        };

        // Act
        await this.Mediator.Send(command);


        // Assert
        this.ArrangeAssertDbContext.FuturesOrders.Single().ToDomainObject().Should().BeEquivalentTo(updatedLimitOrder);
    }
    
    [Theory]
    [ClassData(typeof(ValidPositionAndOrdersGenerator))]
    public async Task UpdateOrderCommand_ShouldUpdateOrderToPositionOpeningOrder_WhenPositionIdIsSpecified(FuturesPosition position, List<FuturesOrder> orders)
    {
        // Arrange
        await this.Mediator.Send(new CreatePositionCommand
        {
            Position = position,
            FuturesOrders = orders,
        });
        
        var index = Random.Shared.Next(orders.Count);
        var order = orders[index];
        var updatedOrder = this.FuturesOrdersGenerator.Clone()
            .RuleFor(x => x.BybitID, order.BybitID)
            .Generate($"default, {order.Type.ToRuleSetName()}, {order.Status.ToRuleSetName()}, {order.Side.ToRuleSetName()}, {order.PositionSide.ToRuleSetName()}");

        
        // Act
        await this.Mediator.Send(new UpdateOrderCommand
        {
            UpdatedOrder = updatedOrder,
            FuturesPositionId = position.CryptoAutopilotId
        });


        // Assert
        orders[index] = updatedOrder;
        this.ArrangeAssertDbContext.FuturesOrders.Select(x => x.ToDomainObject()).Should().BeEquivalentTo(orders);
        this.ArrangeAssertDbContext.FuturesPositions.Single().ToDomainObject().Should().BeEquivalentTo(position);
    }


    [Theory]
    [MemberData(nameof(LimitOrdersGenerator.GetRuleSetsAsObjectArrays), MemberType = typeof(LimitOrdersGenerator))]
    public async Task UpdateOrderCommand_ShouldThrow_WhenNewOrderIsLimitAndPositionIdIsSpecified(string ruleSet)
    {
        // Arrange
        var limitOrder = this.FuturesOrdersGenerator.Generate($"default, {ruleSet}");
        await this.Mediator.Send(new CreateLimitOrderCommand
        {
            LimitOrder = limitOrder
        });

        var updatedLimitOrder = this.FuturesOrdersGenerator.RuleFor(x => x.BybitID, limitOrder.BybitID).Generate($"default, {ruleSet}");
        var command = new UpdateOrderCommand
        {
            UpdatedOrder = updatedLimitOrder,
            FuturesPositionId = Guid.NewGuid()
        };


        // Act
        var func = async () => await this.Mediator.Send(command);

        
        // Assert
        (await func.Should().ThrowExactlyAsync<FluentValidation.ValidationException>())
            .And.Errors.Single().ErrorMessage.Should().Be("The FuturesPositionId must be null when the updated order isn't a position opening order.");
    }


    [Theory]
    [ClassData(typeof(ValidPositionAndOrdersGenerator))]
    public async Task UpdateOrderCommand_ShouldThrow_WhenNewOrderIsPositionOpeningOrderAndIdIsNotSpecified(FuturesPosition position, List<FuturesOrder> orders)
    {
        // Arrange
        await this.Mediator.Send(new CreatePositionCommand
        {
            Position = position,
            FuturesOrders = orders,
        });

        var index = Random.Shared.Next(orders.Count);
        var order = orders[index];
        var updatedOrder = this.FuturesOrdersGenerator.Clone()
            .RuleFor(x => x.BybitID, order.BybitID)
            .Generate($"default, {order.Type.ToRuleSetName()}, {order.Status.ToRuleSetName()}, {order.Side.ToRuleSetName()}, {order.PositionSide.ToRuleSetName()}");

        
        // Act
        var func = async () => await this.Mediator.Send(new UpdateOrderCommand
        {
            UpdatedOrder = updatedOrder
        });

        
        // Assert
        (await func.Should().ThrowExactlyAsync<FluentValidation.ValidationException>())
            .And.Errors.Single().ErrorMessage.Should().Be("The FuturesPositionId can't be null when the updated order is a position opening order.");
    }
}
