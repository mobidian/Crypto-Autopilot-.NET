using Application.Data.Mapping;

using Bybit.Net.Enums;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;
using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Extensions;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.PositionsTests;

public class AddFuturesPositionTests : FuturesTradesDBServiceTestsBase
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
        this.DbContext.FuturesPositions.Single().ToDomainObject().Should().BeEquivalentTo(position);
        this.DbContext.FuturesOrders.Select(x => x.ToDomainObject()).Should().BeEquivalentTo(orders);
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
        (await func.Should().ThrowExactlyAsync<FluentValidation.ValidationException>()).And
                .Errors.Should().ContainSingle(error => error.ErrorMessage == "All orders must have opened a position");
    }

    [Test]
    public async Task AddFuturesPosition_ShouldThrow_WhenThePositionSideOfTheAnyOrderDoesNotMatchTheSideOfThePosition()
    {
        // Arrange
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSide.Buy.ToRuleSetName()}");
        var orders = this.FuturesOrdersGenerator.Generate(10, $"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}, {PositionSide.Sell.ToRuleSetName()}");

        // Act
        var func = async () => await this.SUT.AddFuturesPositionAsync(position, orders);

        // Assert
        (await func.Should().ThrowExactlyAsync<FluentValidation.ValidationException>()).And
                .Errors.Should().ContainSingle(error => error.ErrorMessage == "All orders position side must match the side of the position");
    }
}
