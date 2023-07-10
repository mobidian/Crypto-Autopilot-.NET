using Application.Data.Mapping;

using Bybit.Net.Enums;

using Domain.Models.Futures;

using FluentAssertions;

using Infrastructure.Tests.Integration.Common.Fixtures;
using Infrastructure.Tests.Integration.DataAccess.Extensions;
using Infrastructure.Tests.Integration.DataAccess.FuturesOperationsServiceTests.AbstractBase;

using Xunit;

namespace Infrastructure.Tests.Integration.DataAccess.FuturesOperationsServiceTests;

public class UpdateFuturesPositionsAndAddTheirOrdersTests : FuturesOperationsServiceTestsBase
{
    public UpdateFuturesPositionsAndAddTheirOrdersTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }


    [Fact]
    public async Task UpdateFuturesPositionsAndAddTheirOrdersAsync_ShouldUpdateFuturesPositionsAndAddTheirOrders_WhenAllPositionsExistsAndAllFuturesOrdersRequirePosition()
    {
        // Arrange
        var positionsOrders = new Dictionary<FuturesPosition, IEnumerable<FuturesOrder>>();
        var updatedPositionsOrders = new Dictionary<FuturesPosition, IEnumerable<FuturesOrder>>();

        for (var i = 0; i < 3; i++)
        {
            var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSide.Buy.ToRuleSetName()}");
            var orders = this.FuturesOrdersGenerator.Generate(10, $"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}, {PositionSide.Buy.ToRuleSetName()}");

            var updatedPosition = this.FuturesPositionsGenerator.Clone().RuleFor(x => x.CryptoAutopilotId, position.CryptoAutopilotId).Generate($"default, {PositionSide.Buy.ToRuleSetName()}");
            var newOrders = this.FuturesOrdersGenerator.Generate(3, $"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Sell.ToRuleSetName()}, {PositionSide.Buy.ToRuleSetName()}");

            await this.SUT.AddFuturesPositionAndOrdersAsync(position, orders);

            positionsOrders.Add(position, orders);
            updatedPositionsOrders.Add(updatedPosition, newOrders);
        }


        // Act
        await this.SUT.UpdateFuturesPositionsAndAddTheirOrdersAsync(updatedPositionsOrders);


        // Assert
        var expectedPositions = updatedPositionsOrders.Keys;
        var expectedOrders = positionsOrders.Values.SelectMany(x => x).Concat(updatedPositionsOrders.Values.SelectMany(x => x));
        this.ArrangeAssertDbContext.FuturesPositions.Select(x => x.ToDomainObject()).Should().BeEquivalentTo(expectedPositions);
        this.ArrangeAssertDbContext.FuturesOrders.Select(x => x.ToDomainObject()).Should().BeEquivalentTo(expectedOrders);
    }
}
