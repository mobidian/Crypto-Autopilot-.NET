using Application.Data.Mapping;

using Bybit.Net.Enums;

using FluentAssertions;

using Infrastructure.Tests.Integration.Common.Fixtures;
using Infrastructure.Tests.Integration.DataAccess.FuturesOperationsServiceTests.AbstractBase;

using Tests.Integration.Common.DataAccess.Extensions;

using Xunit;

namespace Infrastructure.Tests.Integration.DataAccess.FuturesOperationsServiceTests;

public class UpdateFuturesPositionAndAddOrdersTests : FuturesOperationsServiceTestsBase
{
    public UpdateFuturesPositionAndAddOrdersTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }


    [Fact]
    public async Task UpdateFuturesPositionAndAddOrders_ShouldUpdateFuturesPositionAndAddOrders_WhenPositionExistsAndAllFuturesOrdersRequirePosition()
    {
        // Arrange
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSide.Buy.ToRuleSetName()}");
        var orders = this.FuturesOrdersGenerator.Generate(10, $"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}, {PositionSide.Buy.ToRuleSetName()}");

        var updatedPosition = this.FuturesPositionsGenerator.Clone().RuleFor(x => x.CryptoAutopilotId, position.CryptoAutopilotId).Generate($"default, {PositionSide.Buy.ToRuleSetName()}");
        var newOrders = this.FuturesOrdersGenerator.Generate(3, $"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Sell.ToRuleSetName()}, {PositionSide.Buy.ToRuleSetName()}");

        await this.SUT.AddFuturesPositionAndOrdersAsync(position, orders);


        // Act
        await this.SUT.UpdateFuturesPositionAndAddOrdersAsync(updatedPosition, newOrders);


        // Assert
        this.ArrangeAssertDbContext.FuturesPositions.Single().ToDomainObject().Should().BeEquivalentTo(updatedPosition);
        this.ArrangeAssertDbContext.FuturesOrders.Select(x => x.ToDomainObject()).Should().BeEquivalentTo(orders.Concat(newOrders));
    }
}
