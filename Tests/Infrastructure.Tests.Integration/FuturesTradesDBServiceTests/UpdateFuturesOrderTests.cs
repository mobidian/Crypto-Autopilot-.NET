using Application.Data.Mapping;

using Domain.Models;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests;

public class UpdateFuturesOrderTests : FuturesTradesDBServiceTestsBase
{
    [Test]
    [TestCaseSource(nameof(GetRuleSetsForLimitOrders))]
    public async Task UpdateFuturesOrderWithoutPositionGuid_ShouldUpdateFuturesOrder_WhenOrderShouldNotPointToPosition(string ruleSets)
    {
        // Arrange
        var order = this.FuturesOrderGenerator.Generate($"default, {ruleSets}");
        await this.DbContext.FuturesOrders.AddAsync(order.ToDbEntity());
        await this.DbContext.SaveChangesAsync();

        var updatedOrder = this.FuturesOrderGenerator.Clone().RuleFor(x => x.BybitID, order.BybitID).Generate($"default, {ruleSets}");


        // Act
        await this.SUT.UpdateFuturesOrderAsync(updatedOrder.BybitID, updatedOrder);


        // Assert
        this.DbContext.FuturesOrders.Single().ToDomainObject().Should().BeEquivalentTo(updatedOrder);
    }

    [Test]
    [TestCaseSource(nameof(GetRuleSetsForLimitOrders))]
    public async Task UpdateFuturesOrderWithoutPositionGuid_ShouldThrow_WhenOrderRequiresPosition(string ruleSets)
    {
        // Arrange
        var order = this.FuturesOrderGenerator.Generate($"default, {ruleSets}");
        await this.DbContext.FuturesOrders.AddAsync(order.ToDbEntity());
        await this.DbContext.SaveChangesAsync();

        var updatedOrder = this.FuturesOrderGenerator.Clone()
            .RuleFor(x => x.BybitID, order.BybitID)
            .RuleFor(x => x.BybitID, order.BybitID)
            .Generate($"default, {this.Faker.PickRandom(GetRuleSetsForMarketOrders())}");

        
        // Act
        var func = async () => await this.SUT.UpdateFuturesOrderAsync(updatedOrder.BybitID, updatedOrder);


        // Assert
        (await func.Should()
            .ThrowExactlyAsync<DbUpdateException>().WithMessage("An error occurred while saving the entity changes. See the inner exception for details."))
            .WithInnerException<ArgumentException>().WithMessage("A created market order or a filled limit order needs to be associated with a position and no position identifier has been specified.")
            .WithInnerExceptionExactly<FluentValidation.ValidationException>();
    }


    [Test]
    [TestCaseSource(nameof(GetRuleSetsForMarketOrders))]
    public async Task UpdateFuturesOrderWithPositionGuid_ShouldUpdateFuturesOrder_WhenOrderRequiresPosition(string ruleSets)
    {
        // Arrange
        var order = this.FuturesOrderGenerator.Generate($"default, {ruleSets}");
        var position = this.FuturesPositionsGenerator.Generate($"default, {ruleSets.Split(',').Last().Trim()}");
        await InsertRelatedPositionAndOrderAsync(position, order);

        var updatedOrder = this.FuturesOrderGenerator.Clone().RuleFor(x => x.BybitID, order.BybitID).Generate($"default, {ruleSets}");


        // Act
        await this.SUT.UpdateFuturesOrderAsync(updatedOrder.BybitID, updatedOrder, position.CryptoAutopilotId);


        // Assert
        this.DbContext.FuturesOrders.Single().ToDomainObject().Should().BeEquivalentTo(updatedOrder);
    }
    private async Task InsertRelatedPositionAndOrderAsync(FuturesPosition position, FuturesOrder order)
    {
        using var transaction = await this.DbContext.Database.BeginTransactionAsync();

        await this.DbContext.FuturesPositions.AddAsync(position.ToDbEntity());
        await this.DbContext.SaveChangesAsync();

        var orderEntity = order.ToDbEntity();
        orderEntity.Position = this.DbContext.FuturesPositions.Single(x => x.CryptoAutopilotId == position.CryptoAutopilotId);
        await this.DbContext.FuturesOrders.AddAsync(orderEntity);
        await this.DbContext.SaveChangesAsync();

        await transaction.CommitAsync();
    }

    [Test]
    [TestCaseSource(nameof(GetRuleSetsForLimitOrders))]
    public async Task UpdateFuturesOrderWithPositionGuid_ShouldThrow_WhenOrderShouldNotPointToPosition(string ruleSets)
    {
        // Arrange
        var order = this.FuturesOrderGenerator.Generate($"default, {ruleSets}");
        var position = this.FuturesPositionsGenerator.Generate($"default, {ruleSets.Split(',').Last().Trim()}");

        await this.DbContext.FuturesOrders.AddAsync(order.ToDbEntity());
        await this.DbContext.FuturesPositions.AddAsync(position.ToDbEntity());
        await this.DbContext.SaveChangesAsync();

        var updatedOrder = this.FuturesOrderGenerator.Clone().RuleFor(x => x.BybitID, order.BybitID).Generate($"default, {ruleSets}");


        // Act
        var func = async () => await this.SUT.UpdateFuturesOrderAsync(updatedOrder.BybitID, updatedOrder, position.CryptoAutopilotId);


        // Assert
        (await func.Should()
            .ThrowExactlyAsync<DbUpdateException>().WithMessage("An error occurred while saving the entity changes. See the inner exception for details."))
            .WithInnerException<ArgumentException>().WithMessage("Only a created market order or a filled limit order can be associated with a position.")
            .WithInnerExceptionExactly<FluentValidation.ValidationException>();
    }
}
