using Application.Data.Mapping;

using Bybit.Net.Enums;

using Infrastructure.Tests.Integration.DataAccess.Extensions;
using Infrastructure.Tests.Integration.DataAccess.FuturesPositionsRepositoryTests.AbstractBase;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Integration.DataAccess.FuturesPositionsRepositoryTests;

public class AddFuturesPositionTests : FuturesPositionsRepositoryTestsBase
{
    [Test]
    public async Task AddFuturesPosition_ShouldAddFuturesPositionAndOrders_WhenAllFuturesOrdersRequirePosition()
    {
        // Arrange
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSide.Buy.ToRuleSetName()}");
        var orders = this.FuturesOrdersGenerator.Generate(10, $"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}, {PositionSide.Buy.ToRuleSetName()}");

        // Act
        await this.SUT.AddFuturesPositionAsync(position, orders);

        // Assert
        this.ArrangeAssertDbContext.FuturesPositions.Single().ToDomainObject().Should().BeEquivalentTo(position);
        this.ArrangeAssertDbContext.FuturesOrders.Select(x => x.ToDomainObject()).Should().BeEquivalentTo(orders);
    }

    [Test]
    public async Task AddFuturesPosition_ShouldThrow_WhenAnyFuturesOrderDoesNotRequirePosition()
    {
        // Arrange
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSide.Buy.ToRuleSetName()}");
        var orders = new[]
        {
            this.FuturesOrdersGenerator.Generate($"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}"),
            this.FuturesOrdersGenerator.Generate($"default, {OrderType.Limit.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}"), // does not need to point to position
        };

        // Act
        var func = async () => await this.SUT.AddFuturesPositionAsync(position, orders);

        // Assert
        (await func.Should()
            .ThrowExactlyAsync<DbUpdateException>()
            .WithMessage("An error occurred while validating relationships between entities. The database update operation cannot be performed."))
                .WithInnerExceptionExactly<FluentValidation.ValidationException>()
                .And.Errors.Should().ContainSingle(error => error.ErrorMessage == "A limit order which has not been filled must not point to a position.");
    }

    [Test]
    public async Task AddFuturesPosition_ShouldThrow_WhenThePositionSideOfAnyOrderDoesNotMatchTheSideOfThePosition()
    {
        // Arrange
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSide.Buy.ToRuleSetName()}");
        var orders = this.FuturesOrdersGenerator.Generate(10, $"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}, {PositionSide.Sell.ToRuleSetName()}");

        // Act
        var func = async () => await this.SUT.AddFuturesPositionAsync(position, orders);

        // Assert
        (await func.Should()
            .ThrowExactlyAsync<DbUpdateException>()
            .WithMessage("An error occurred while validating relationships between entities. The database update operation cannot be performed."))
                .WithInnerExceptionExactly<FluentValidation.ValidationException>()
                .And.Errors.Should().ContainSingle(error => error.ErrorMessage == "An order which points to a position must have the appropriate value for the PositionSide property.");
    }
}
