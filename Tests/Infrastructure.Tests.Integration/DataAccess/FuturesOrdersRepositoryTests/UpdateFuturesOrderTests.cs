using Application.Data.Mapping;

using Bybit.Net.Enums;

using FluentAssertions;

using Infrastructure.Tests.Integration.Common.Fixtures;
using Infrastructure.Tests.Integration.DataAccess.Extensions;
using Infrastructure.Tests.Integration.DataAccess.FuturesOrdersRepositoryTests.AbstractBase;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace Infrastructure.Tests.Integration.DataAccess.FuturesOrdersRepositoryTests;

public class UpdateFuturesOrderTests : FuturesOrdersRepositoryTestsBase
{
    public UpdateFuturesOrderTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }


    [Fact]
    public async Task UpdateFuturesOrderWithoutPositionGuid_ShouldUpdateFuturesOrder_WhenOrderShouldNotPointToPosition()
    {
        // Arrange
        var order = this.FuturesOrdersGenerator.Generate($"default, {OrderType.Limit.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}");
        await this.ArrangeAssertDbContext.FuturesOrders.AddAsync(order.ToDbEntity());
        await this.ArrangeAssertDbContext.SaveChangesAsync();

        var updatedOrder = this.FuturesOrdersGenerator.Clone().RuleFor(x => x.BybitID, order.BybitID).Generate($"default, {OrderType.Limit.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}");


        // Act
        await this.SUT.UpdateAsync(updatedOrder.BybitID, updatedOrder);


        // Assert
        this.ArrangeAssertDbContext.FuturesOrders.Single().ToDomainObject().Should().BeEquivalentTo(updatedOrder);
    }

    [Fact]
    public async Task UpdateFuturesOrderWithPositionGuid_ShouldUpdateFuturesOrder_WhenOrderRequiresPosition()
    {
        // Arrange
        var order = this.FuturesOrdersGenerator.Generate($"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}, {PositionSide.Buy.ToRuleSetName()}");
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSide.Buy.ToRuleSetName()}");
        await InsertRelatedPositionAndOrdersAsync(position, new[] { order });

        var updatedOrder = this.FuturesOrdersGenerator.Clone().RuleFor(x => x.BybitID, order.BybitID).Generate($"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}, {PositionSide.Buy.ToRuleSetName()}");


        // Act
        await this.SUT.UpdateAsync(updatedOrder.BybitID, updatedOrder, position.CryptoAutopilotId);


        // Assert
        this.ArrangeAssertDbContext.FuturesOrders.Single().ToDomainObject().Should().BeEquivalentTo(updatedOrder);
    }

    [Fact]
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
        var func = async () => await this.SUT.UpdateAsync(updatedOrder.BybitID, updatedOrder);

        
        // Assert
        (await func.Should()
            .ThrowExactlyAsync<DbUpdateException>()
            .WithMessage("An error occurred while validating the entities. The database update operation cannot be performed."))
                .WithInnerExceptionExactly<DbUpdateException>()
                .WithMessage("The new order position side property value does not match the side property value of the related position.");
    }
}
