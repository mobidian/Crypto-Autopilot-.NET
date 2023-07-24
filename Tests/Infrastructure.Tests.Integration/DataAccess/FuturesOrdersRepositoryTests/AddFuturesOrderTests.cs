using Application.Data.Mapping;

using Bybit.Net.Enums;

using FluentAssertions;

using Infrastructure.Tests.Integration.Common.Fixtures;
using Infrastructure.Tests.Integration.DataAccess.FuturesOrdersRepositoryTests.AbstractBase;

using Tests.Integration.Common.DataAccess.Extensions;

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
        await this.SUT.AddAsync(order);

        // Assert
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
        await this.SUT.AddAsync(order, position.CryptoAutopilotId);

        // Assert
        this.ArrangeAssertDbContext.FuturesOrders.Single().ToDomainObject().Should().BeEquivalentTo(order);
    }
}
