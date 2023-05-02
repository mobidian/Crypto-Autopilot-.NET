using Application.Data.Mapping;

using Bybit.Net.Enums;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;
using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Extensions;

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
        (await func.Should().ThrowExactlyAsync<FluentValidation.ValidationException>()).And
                .Errors.Should().ContainSingle(error => error.ErrorMessage == "The order can't be a market order or a filled limit order in order, otherwise it would have opened a position");
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
        (await func.Should().ThrowExactlyAsync<FluentValidation.ValidationException>()).And
                .Errors.Should().ContainSingle(error => error.ErrorMessage == "The order must be a market order or a filled limit order in order, otherwise it cannot have opened a position");
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
        (await func.Should().ThrowExactlyAsync<FluentValidation.ValidationException>()).And
                .Errors.Should().ContainSingle(error => error.ErrorMessage == "All orders position side must match the side of the position");
    }
}
