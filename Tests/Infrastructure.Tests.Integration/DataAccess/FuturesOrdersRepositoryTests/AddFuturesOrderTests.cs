using Application.Data.Mapping;

using Bybit.Net.Enums;

using FluentAssertions;

using Infrastructure.Tests.Integration.DataAccess.FuturesOrdersRepositoryTests.AbstractBase;

using Microsoft.EntityFrameworkCore;

using Tests.Integration.Common.DataAccess.Extensions;
using Tests.Integration.Common.Fixtures;

using Xunit;

namespace Infrastructure.Tests.Integration.DataAccess.FuturesOrdersRepositoryTests;

public class AddFuturesOrderTests : FuturesOrdersRepositoryTestsBase
{
    public AddFuturesOrderTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }


    [Fact]
    public async Task AddFuturesOrderWithoutPositionGuid_ShouldAddFuturesOrder_WhenOrderShouldNotPointToPosition()
    {
        // Arrange
        var order = this.FuturesOrdersGenerator.Generate($"default, {OrderType.Limit.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}");

        // Act
        var added = await this.SUT.AddAsync(order);

        // Assert
        added.Should().BeTrue();
        this.ArrangeAssertDbContext.FuturesOrders.Single().ToDomainObject().Should().BeEquivalentTo(order);
    }
    
    [Fact]
    public async Task AddFuturesOrderWithPositionGuid_ShouldAddFuturesOrder_WhenOrderRequiresPosition()
    {
        // Arrange
        var order = this.FuturesOrdersGenerator.Generate($"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}, {PositionSide.Buy.ToRuleSetName()}");
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSide.Buy.ToRuleSetName()}");
        await this.ArrangeAssertDbContext.FuturesPositions.AddAsync(position.ToDbEntity());
        await this.ArrangeAssertDbContext.SaveChangesAsync();

        // Act
        var added = await this.SUT.AddAsync(order, position.CryptoAutopilotId);

        // Assert
        added.Should().BeTrue();
        this.ArrangeAssertDbContext.FuturesOrders.Single().ToDomainObject().Should().BeEquivalentTo(order);
    }

    [Fact]
    public async Task AddFuturesOrderWithPositionGuid_ShouldThrow_WhenThePositionSideDoesNotMatchTheOrderSide()
    {
        // Arrange
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSide.Buy.ToRuleSetName()}");
        var order = this.FuturesOrdersGenerator.Generate($"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}, {PositionSide.Sell.ToRuleSetName()}");

        await this.ArrangeAssertDbContext.FuturesPositions.AddAsync(position.ToDbEntity());
        await this.ArrangeAssertDbContext.SaveChangesAsync();


        // Act
        var func = async () => await this.SUT.AddAsync(order, position.CryptoAutopilotId);


        // Assert
        (await func.Should()
            .ThrowExactlyAsync<DbUpdateException>()
            .WithMessage("An error occurred while validating the entities. The database update operation cannot be performed."))
                .WithInnerExceptionExactly<DbUpdateException>()
                .WithMessage("The added order position side property value does not match the side property value of the related position.");
    }
}
