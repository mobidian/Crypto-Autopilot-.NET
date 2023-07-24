using Application.Data.Mapping;

using Bybit.Net.Enums;

using FluentAssertions;

using Infrastructure.Tests.Integration.Common.Fixtures;
using Infrastructure.Tests.Integration.DataAccess.FuturesOrdersRepositoryTests.AbstractBase;

using Tests.Integration.Common.DataAccess.Extensions;

using Xunit;

namespace Infrastructure.Tests.Integration.DataAccess.FuturesOrdersRepositoryTests;

public class AddFuturesOrdersTests : FuturesOrdersRepositoryTestsBase
{
    public AddFuturesOrdersTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }


    [Fact]
    public async Task AddFuturesOrdersWithoutPositionGuid_ShouldAddFuturesOrders_WhenAllOrdersShouldNotPointToPosition()
    {
        // Arrange
        var orders = this.FuturesOrdersGenerator.Generate(10, $"default, {OrderType.Limit.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}");

        // Act
        await this.SUT.AddAsync(orders);

        // Assert
        this.ArrangeAssertDbContext.FuturesOrders.Select(x => x.ToDomainObject()).Should().BeEquivalentTo(orders);
    }

    [Fact]
    public async Task AddFuturesOrderWithPositionGuid_ShouldAddFuturesOrders_WhenAllFuturesOrdersRequirePosition()
    {
        // Arrange
        var orders = this.FuturesOrdersGenerator.Generate(10, $"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}, {PositionSide.Buy.ToRuleSetName()}");
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSide.Buy.ToRuleSetName()}");
        await this.ArrangeAssertDbContext.FuturesPositions.AddAsync(position.ToDbEntity());
        await this.ArrangeAssertDbContext.SaveChangesAsync();

        // Act
        await this.SUT.AddAsync(orders, position.CryptoAutopilotId);

        // Assert
        this.ArrangeAssertDbContext.FuturesOrders.Select(x => x.ToDomainObject()).Should().BeEquivalentTo(orders);
    }
}
