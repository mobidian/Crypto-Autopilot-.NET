using Application.Data.Mapping;

using Bybit.Net.Enums;

using FluentAssertions;

using Infrastructure.Tests.Integration.Common.Fixtures;
using Infrastructure.Tests.Integration.DataAccess.Extensions;
using Infrastructure.Tests.Integration.DataAccess.FuturesPositionsRepositoryTests.AbstractBase;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace Infrastructure.Tests.Integration.DataAccess.FuturesPositionsRepositoryTests;

public class UpdateFuturesPositionTests : FuturesPositionsRepositoryTestsBase
{
    public UpdateFuturesPositionTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }


    [Fact]
    public async Task UpdateFuturesPosition_ShouldUpdateFuturesPosition_WhenFuturesPositionIsValid()
    {
        // Arrange
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSide.Buy.ToRuleSetName()}");
        var orders = this.FuturesOrdersGenerator.Generate(10, $"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}, {PositionSide.Buy.ToRuleSetName()}");
        await InsertRelatedPositionAndOrdersAsync(position, orders);

        var updatedPosition = this.FuturesPositionsGenerator.Clone()
            .RuleFor(x => x.CryptoAutopilotId, position.CryptoAutopilotId)
            .Generate($"default, {PositionSide.Buy.ToRuleSetName()}");


        // Act
        await this.SUT.UpdateFuturesPositionAsync(updatedPosition.CryptoAutopilotId, updatedPosition);

        // Assert
        this.ArrangeAssertDbContext.FuturesPositions.Single().ToDomainObject().Should().BeEquivalentTo(updatedPosition);
    }

    [Fact]
    public async Task UpdateFuturesPosition_ShouldThrow_WhenFuturesPositionSideDoesNotMatchTheOldFuturesPositionSideAndThePositionAlreadyHasOrdersPointingToIt()
    {
        // Arrange
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSide.Buy.ToRuleSetName()}");
        var orders = this.FuturesOrdersGenerator.Generate(10, $"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}, {PositionSide.Buy.ToRuleSetName()}");
        await InsertRelatedPositionAndOrdersAsync(position, orders);

        var updatedPosition = this.FuturesPositionsGenerator.Clone()
            .RuleFor(x => x.CryptoAutopilotId, position.CryptoAutopilotId)
            .Generate($"default, {PositionSide.Sell.ToRuleSetName()}");


        // Act
        var func = async () => await this.SUT.UpdateFuturesPositionAsync(updatedPosition.CryptoAutopilotId, updatedPosition);

        
        // Assert
        (await func.Should()
            .ThrowExactlyAsync<DbUpdateException>()
            .WithMessage("An error occurred while validating the entities. The database update operation cannot be performed."))
                .WithInnerExceptionExactly<DbUpdateException>()
                .WithMessage("The new position side property value does not match the position side property value of the related orders.");
    }
}
