using Application.Data.Mapping;

using Bybit.Net.Enums;

using Domain.Commands.Orders;
using Domain.Commands.Positions;

using FluentAssertions;

using Infrastructure.Tests.Integration.BusinessLogic.Commands.AbstractBase;
using Infrastructure.Tests.Integration.Common.Fixtures;

using Microsoft.EntityFrameworkCore;

using Tests.Integration.Common.DataAccess.Extensions;

using Xunit;

namespace Infrastructure.Tests.Integration.BusinessLogic.Commands.OrdersTests;

public class DeleteOrdersCommandTests : CommandsTestsBase
{
    public DeleteOrdersCommandTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    
    [Fact]
    public async Task DeleteOrdersCommand_ShouldDeleteOrders_WhenSpecifiedIdsExist()
    {
        // Arrange
        var orders = this.FuturesOrdersGenerator.Generate(1000, $"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}, {PositionSide.Buy.ToRuleSetName()}");
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSide.Buy.ToRuleSetName()}");

        await this.Mediator.Send(new CreatePositionCommand
        {
            Position = position,
            FuturesOrders = orders,
        });
        
        var command = new DeleteOrdersCommand
        {
            BybitIds = orders.Select(x => x.BybitID).ToArray()
        };


        // Act
        await this.Mediator.Send(command);

        
        // Assert
        this.ArrangeAssertDbContext.FuturesOrders.Should().BeEmpty();
        this.ArrangeAssertDbContext.FuturesPositions.Single().ToDomainObject().Should().BeEquivalentTo(position);
    }

    [Fact]
    public async Task DeleteOrdersCommand_ShouldThrow_WhenSpecifiedIdDoesNotExist()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var command = new DeleteOrdersCommand
        {
            BybitIds = new[] { guid }
        };
        
        // Act
        var func = async () => await this.Mediator.Send(command);
        
        // Assert
        (await func.Should().ThrowExactlyAsync<DbUpdateException>())
            .WithMessage($"No order with bybitID {guid} was found in the database");
    }
}
