using Application.Data.Mapping;

using Bybit.Net.Enums;

using FluentAssertions;

using Infrastructure.Tests.Integration.Common.Fixtures;
using Infrastructure.Tests.Integration.DataAccess.Extensions;
using Infrastructure.Tests.Integration.DataAccess.FuturesOperationsUnitOfWorkTests.AbstractBase;

using Xunit;

namespace Infrastructure.Tests.Integration.DataAccess.FuturesOperationsUnitOfWorkTests;

public class AddFuturesPositionAndOrdersTests : FuturesOperationsServiceTestsBase
{
    public AddFuturesPositionAndOrdersTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }


    [Fact]
    public async Task AddFuturesPosition_ShouldAddFuturesPositionAndOrders_WhenAllFuturesOrdersRequirePosition()
    {
        // Arrange
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSide.Buy.ToRuleSetName()}");
        var orders = this.FuturesOrdersGenerator.Generate(10, $"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}, {PositionSide.Buy.ToRuleSetName()}");

        // Act
        await this.SUT.AddFuturesPositionAndOrdersAsync(position, orders);

        // Assert
        this.ArrangeAssertDbContext.FuturesPositions.Single().ToDomainObject().Should().BeEquivalentTo(position);
        this.ArrangeAssertDbContext.FuturesOrders.Select(x => x.ToDomainObject()).Should().BeEquivalentTo(orders);
    }
}
