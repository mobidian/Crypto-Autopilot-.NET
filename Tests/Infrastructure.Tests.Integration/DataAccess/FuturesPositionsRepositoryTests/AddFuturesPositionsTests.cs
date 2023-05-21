﻿using Application.Data.Mapping;

using Bybit.Net.Enums;

using Infrastructure.Tests.Integration.DataAccess.Extensions;
using Infrastructure.Tests.Integration.DataAccess.FuturesPositionsRepositoryTests.AbstractBase;

using Microsoft.Data.SqlClient;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Integration.DataAccess.FuturesPositionsRepositoryTests;

public class AddFuturesPositionsTests : FuturesPositionsRepositoryTestsBase
{
    [Test]
    public async Task AddFuturesPositions_ShouldAddFuturesPositions_WhenFuturesPositionsAreValid()
    {
        // Arrange
        var positions = this.FuturesPositionsGenerator.Generate(5, $"default, {PositionSide.Buy.ToRuleSetName()}");

        // Act
        await this.SUT.AddFuturesPositionsAsync(positions);

        // Assert
        this.ArrangeAssertDbContext.FuturesPositions.Select(x => x.ToDomainObject()).Should().BeEquivalentTo(positions);
    }

    [Test]
    public async Task AddFuturesPositions_ShouldThrow_WhenAnyFuturesPositionIsInvalid()
    {
        // Arrange
        var validPosition = this.FuturesPositionsGenerator.Generate();
        var invalidPosition = this.FuturesPositionsGenerator.Clone().RuleFor(x => x.Leverage, 0).Generate();
        var positions = new[] { validPosition, invalidPosition };

        // Act
        var func = async () => await this.SUT.AddFuturesPositionsAsync(positions);

        // Assert
        (await func.Should()
            .ThrowExactlyAsync<DbUpdateException>()
            .WithMessage("An error occurred while validating the entities. The database update operation cannot be performed."))
                .WithInnerExceptionExactly<FluentValidation.ValidationException>();
    }

    [Test]
    public async Task AddFuturesPositions_ShouldThrow_WhenCryptoAutopilotIdAlreadyExistsInTheDatabase()
    {
        // Arrange
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSide.Buy.ToRuleSetName()}");
        await this.SUT.AddFuturesPositionAsync(position);

        var cryptoAutopilotId = position.CryptoAutopilotId;
        var positionWithSameCryptoAutopilotId = this.FuturesPositionsGenerator.Clone().RuleFor(x => x.CryptoAutopilotId, cryptoAutopilotId).Generate();
        var positions = this.FuturesPositionsGenerator.Generate(10, $"default, {PositionSide.Buy.ToRuleSetName()}").Append(positionWithSameCryptoAutopilotId);


        // Act
        var func = async () => await this.SUT.AddFuturesPositionsAsync(positions);


        // Assert
        (await func.Should()
            .ThrowExactlyAsync<DbUpdateException>()
            .WithMessage("An error occurred while saving the entity changes. See the inner exception for details."))
                .WithInnerExceptionExactly<SqlException>()
                .WithMessage($"""Cannot insert duplicate key row in object 'dbo.FuturesPositions' with unique index 'IX_FuturesPositions_CryptoAutopilotId'. The duplicate key value is ({cryptoAutopilotId}).""");
    }
}