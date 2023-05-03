﻿using Application.Data.Mapping;

using Bybit.Net.Enums;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;
using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Extensions;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.PositionsTests;

public class UpdateFuturesPositionTests : FuturesTradesDBServiceTestsBase
{
    [Test]
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
        this.DbContext.FuturesPositions.Single().ToDomainObject().Should().BeEquivalentTo(updatedPosition);
    }

    [Test]
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
            .WithMessage("An error occurred while validating relationships between entities. The database update operation cannot be performed."))
                .WithInnerExceptionExactly<FluentValidation.ValidationException>()
                .And.Errors.Should().ContainSingle(error => error.ErrorMessage == "An order which points to a position must have the appropriate value for the PositionSide property.");
    }
}
