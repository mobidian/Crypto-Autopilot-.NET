using Application.Data.Mapping;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.OrdersTests;

public class AddFuturesOrdersTests : FuturesTradesDBServiceTestsBase
{
    [Test]
    public async Task AddFuturesOrdersWithoutPositionGuid_ShouldAddFuturesOrders_WhenAllOrdersShouldNotPointToPosition()
    {
        // Arrange
        var orders = this.FuturesOrderGenerator.Generate(10, $"default, {LimitOrder}, {SideBuy}");

        // Act
        await this.SUT.AddFuturesOrdersAsync(orders);

        // Assert
        this.DbContext.FuturesOrders.Select(x => x.ToDomainObject()).Should().BeEquivalentTo(orders);
    }

    [Test]
    public async Task AddFuturesOrdersWithoutPositionGuid_ShouldThrow_WhenAnyFuturesOrderRequiresPosition()
    {
        // Arrange
        var orders = this.FuturesOrderGenerator.Generate(10, $"default, {LimitOrder}, {SideBuy}");
        orders.Add(this.FuturesOrderGenerator.Generate($"default, {MarketOrder}, {SideBuy}")); // requires position

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
        var orders = this.FuturesOrderGenerator.Generate(10, $"default, {MarketOrder}, {SideBuy}, {OrderPositionLong}");
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSideLong}");
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
        var orders = this.FuturesOrderGenerator.Generate(10, $"default, {MarketOrder}, {SideBuy}, {OrderPositionLong}");
        orders.Add(this.FuturesOrderGenerator.Generate($"default, {LimitOrder}, {SideBuy}, {OrderPositionLong}")); // does not require position

        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSideLong}");
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
            this.FuturesOrderGenerator.Generate($"default, {MarketOrder}, {SideBuy}, {OrderPositionLong}"),
            this.FuturesOrderGenerator.Generate($"default, {MarketOrder}, {SideBuy}, {OrderPositionShort}"),
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
        var orders = this.FuturesOrderGenerator.Generate(10, $"default, {MarketOrder}, {SideBuy}, {OrderPositionLong}");
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSideShort}");
        await this.DbContext.FuturesPositions.AddAsync(position.ToDbEntity());
        await this.DbContext.SaveChangesAsync();

        // Act
        var func = async () => await this.SUT.AddFuturesOrdersAsync(orders, position.CryptoAutopilotId);

        // Assert
        (await func.Should().ThrowExactlyAsync<FluentValidation.ValidationException>()).And
                .Errors.Should().ContainSingle(error => error.ErrorMessage == "All orders position side must match the side of the position");
    }
}
