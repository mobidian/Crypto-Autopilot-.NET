using Application.Data.Mapping;

using Bybit.Net.Enums;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;
using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Extensions;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.OrdersTests;

public class AddFuturesOrderTests : FuturesTradesDBServiceTestsBase
{
    [Test]
    public async Task AddFuturesOrderWithoutPositionGuid_ShouldAddFuturesOrder_WhenOrderShouldNotPointToPosition()
    {
        // Arrange
        var order = this.FuturesOrdersGenerator.Generate($"default, {OrderType.Limit.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}");

        // Act
        await this.SUT.AddFuturesOrderAsync(order);

        // Assert
        this.DbContext.FuturesOrders.Single().ToDomainObject().Should().BeEquivalentTo(order);
    }

    [Test]
    public async Task AddFuturesOrderWithoutPositionGuid_ShouldThrow_WhenOrderRequiresPosition()
    {
        // Arrange
        var order = this.FuturesOrdersGenerator.Generate($"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}");

        // Act
        var func = async () => await this.SUT.AddFuturesOrderAsync(order);

        // Assert
        (await func.Should()
            .ThrowExactlyAsync<DbUpdateException>()
            .WithMessage("An error occurred while validating relationships between entities. The database update operation cannot be performed."))
                .WithInnerExceptionExactly<FluentValidation.ValidationException>()
                .And.Errors.Should().ContainSingle(error => error.ErrorMessage == "A market order must point to a position.");
    }


    [Test]
    public async Task AddFuturesOrderWithPositionGuid_ShouldAddFuturesOrder_WhenOrderRequiresPosition()
    {
        // Arrange
        var order = this.FuturesOrdersGenerator.Generate($"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}, {PositionSide.Buy.ToRuleSetName()}");
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSide.Buy.ToRuleSetName()}");
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
        var order = this.FuturesOrdersGenerator.Generate($"default, {OrderType.Limit.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}, {PositionSide.Buy.ToRuleSetName()}");
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSide.Buy.ToRuleSetName()}");
        await this.DbContext.FuturesPositions.AddAsync(position.ToDbEntity());
        await this.DbContext.SaveChangesAsync();

        // Act
        var func = async () => await this.SUT.AddFuturesOrderAsync(order, position.CryptoAutopilotId);

        // Assert
        (await func.Should()
            .ThrowExactlyAsync<DbUpdateException>()
            .WithMessage("An error occurred while validating relationships between entities. The database update operation cannot be performed."))
                .WithInnerExceptionExactly<FluentValidation.ValidationException>()
                .And.Errors.Should().ContainSingle(error => error.ErrorMessage == "A limit order which has not been filled must not point to a position.");
    }


    [Test]
    public async Task AddFuturesOrderWithPositionGuid_ShouldThrow_WhenTheOrderPositionSideDoesNotMatchTheSideOfThePosition()
    {
        // Arrange
        var order = this.FuturesOrdersGenerator.Generate($"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}, {PositionSide.Buy.ToRuleSetName()}");
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSide.Sell.ToRuleSetName()}");
        await this.DbContext.FuturesPositions.AddAsync(position.ToDbEntity());
        await this.DbContext.SaveChangesAsync();

        // Act
        var func = async () => await this.SUT.AddFuturesOrderAsync(order, position.CryptoAutopilotId);

        // Assert
        (await func.Should()
            .ThrowExactlyAsync<DbUpdateException>()
            .WithMessage("An error occurred while validating relationships between entities. The database update operation cannot be performed."))
                .WithInnerExceptionExactly<FluentValidation.ValidationException>()
                .And.Errors.Should().ContainSingle(error => error.ErrorMessage == "An order which points to a position must have the appropriate value for the PositionSide property.");
    }
}
