using Application.Data.Mapping;

using Bybit.Net.Enums;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;
using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Extensions;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.OrdersTests;

public class AddFuturesOrdersTests : FuturesTradesDBServiceTestsBase
{
    [Test]
    public async Task AddFuturesOrdersWithoutPositionGuid_ShouldAddFuturesOrders_WhenAllOrdersShouldNotPointToPosition()
    {
        // Arrange
        var orders = this.FuturesOrdersGenerator.Generate(10, $"default, {OrderType.Limit.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}");

        // Act
        await this.SUT.AddFuturesOrdersAsync(orders);

        // Assert
        this.DbContext.FuturesOrders.Select(x => x.ToDomainObject()).Should().BeEquivalentTo(orders);
    }

    [Test]
    public async Task AddFuturesOrdersWithoutPositionGuid_ShouldThrow_WhenAnyFuturesOrderRequiresPosition()
    {
        // Arrange
        var orders = this.FuturesOrdersGenerator.Generate(10, $"default, {OrderType.Limit.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}");
        orders.Add(this.FuturesOrdersGenerator.Generate($"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}")); // requires position

        // Act
        var func = async () => await this.SUT.AddFuturesOrdersAsync(orders);

        // Assert
        (await func.Should().ThrowExactlyAsync<FluentValidation.ValidationException>()).And
                .Errors.Should().ContainSingle(error => error.ErrorMessage == "No order should have opened a position");
    }


    [Test]
    public async Task AddFuturesOrderWithPositionGuid_ShouldAddFuturesOrders_WhenAllFuturesOrdersRequirePosition()
    {
        // Arrange
        var orders = this.FuturesOrdersGenerator.Generate(10, $"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}, {PositionSide.Buy.ToRuleSetName()}");
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSide.Buy.ToRuleSetName()}");
        await this.DbContext.FuturesPositions.AddAsync(position.ToDbEntity());
        await this.DbContext.SaveChangesAsync();

        // Act
        await this.SUT.AddFuturesOrdersAsync(orders, position.CryptoAutopilotId);

        // Assert
        this.DbContext.FuturesOrders.Select(x => x.ToDomainObject()).Should().BeEquivalentTo(orders);
    }

    [Test]
    public async Task AddFuturesOrderWithPositionGuid_ShouldThrow_WhenAnyFuturesOrderDoesNotRequirePosition()
    {
        // Arrange
        var orders = this.FuturesOrdersGenerator.Generate(10, $"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}, {PositionSide.Buy.ToRuleSetName()}");
        orders.Add(this.FuturesOrdersGenerator.Generate($"default, {OrderType.Limit.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}, {PositionSide.Buy.ToRuleSetName()}")); // does not require position

        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSide.Buy.ToRuleSetName()}");
        await this.DbContext.FuturesPositions.AddAsync(position.ToDbEntity());
        await this.DbContext.SaveChangesAsync();


        // Act
        var func = async () => await this.SUT.AddFuturesOrdersAsync(orders, position.CryptoAutopilotId);


        // Assert
        (await func.Should().ThrowExactlyAsync<FluentValidation.ValidationException>()).And
                .Errors.Should().ContainSingle(error => error.ErrorMessage == "All orders must have opened a position");
    }


    [Test]
    public async Task AddFuturesOrder_ShouldThrow_WhenNotAllOrdersHaveTheSamePositionSide()
    {
        // Arrange
        var orders = new[]
        {
            this.FuturesOrdersGenerator.Generate($"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}, {PositionSide.Buy.ToRuleSetName()}"),
            this.FuturesOrdersGenerator.Generate($"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}, {PositionSide.Sell.ToRuleSetName()}"),
        };

        // Act
        var func = async () => await this.SUT.AddFuturesOrdersAsync(orders);

        // Assert
        (await func.Should().ThrowExactlyAsync<FluentValidation.ValidationException>()).And
                .Errors.Should().ContainSingle(error => error.ErrorMessage == "All orders must have the same position side");
    }

    [Test]
    public async Task AddFuturesOrder_ShouldThrow_WhenThePositionSideOfTheOrdersDoesNotMatchThePositionSide()
    {
        // Arrange
        var orders = this.FuturesOrdersGenerator.Generate(10, $"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}, {PositionSide.Buy.ToRuleSetName()}");
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSide.Sell.ToRuleSetName()}");
        await this.DbContext.FuturesPositions.AddAsync(position.ToDbEntity());
        await this.DbContext.SaveChangesAsync();

        // Act
        var func = async () => await this.SUT.AddFuturesOrdersAsync(orders, position.CryptoAutopilotId);

        // Assert
        (await func.Should().ThrowExactlyAsync<FluentValidation.ValidationException>()).And
                .Errors.Should().ContainSingle(error => error.ErrorMessage == "All orders position side must match the side of the position");
    }
}
