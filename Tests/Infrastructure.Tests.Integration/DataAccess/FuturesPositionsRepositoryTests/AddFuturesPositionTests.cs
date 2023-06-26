using Application.Data.Mapping;

using Bybit.Net.Enums;

using FluentAssertions;

using Infrastructure.Tests.Integration.Common.Fixtures;
using Infrastructure.Tests.Integration.DataAccess.Extensions;
using Infrastructure.Tests.Integration.DataAccess.FuturesPositionsRepositoryTests.AbstractBase;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Xunit;

namespace Infrastructure.Tests.Integration.DataAccess.FuturesPositionsRepositoryTests;

public class AddFuturesPositionTests : FuturesPositionsRepositoryTestsBase
{
    public AddFuturesPositionTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }


    [Fact]
    public async Task AddFuturesPosition_ShouldAddFuturesPosition_WhenFuturesPositionIsValid()
    {
        // Arrange
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSide.Buy.ToRuleSetName()}");

        // Act
        await this.SUT.AddAsync(position);

        // Assert
        this.ArrangeAssertDbContext.FuturesPositions.Single().ToDomainObject().Should().BeEquivalentTo(position);
    }

    [Fact]
    public async Task AddFuturesPosition_ShouldThrow_WhenFuturesPositionIsInvalid()
    {
        // Arrange
        var invalidPosition = this.FuturesPositionsGenerator.Clone().RuleFor(x => x.Leverage, 0).Generate();

        // Act
        var func = async () => await this.SUT.AddAsync(invalidPosition);

        // Assert
        (await func.Should()
            .ThrowExactlyAsync<DbUpdateException>()
            .WithMessage("An error occurred while validating the entities. The database update operation cannot be performed."))
                .WithInnerExceptionExactly<FluentValidation.ValidationException>()
                .And.Errors.Should().ContainSingle(error => error.ErrorMessage == "'Leverage' must be greater than or equal to '1'.");
    }

    [Fact]
    public async Task AddFuturesPosition_ShouldThrow_WhenCryptoAutopilotIdAlreadyExistsInTheDatabase()
    {
        // Arrange
        var position = this.FuturesPositionsGenerator.Generate($"default, {PositionSide.Buy.ToRuleSetName()}");
        await this.SUT.AddAsync(position);

        var cryptoAutopilotId = position.CryptoAutopilotId;
        var positionWithSameCryptoAutopilotId = this.FuturesPositionsGenerator.Clone().RuleFor(x => x.CryptoAutopilotId, cryptoAutopilotId).Generate();


        // Act
        var func = async () => await this.SUT.AddAsync(positionWithSameCryptoAutopilotId);
        

        // Assert
        (await func.Should()
            .ThrowExactlyAsync<DbUpdateException>()
            .WithMessage("An error occurred while saving the entity changes. See the inner exception for details."))
                .WithInnerExceptionExactly<SqlException>()
                .WithMessage($"""Cannot insert duplicate key row in object 'dbo.FuturesPositions' with unique index 'IX_FuturesPositions_CryptoAutopilotId'. The duplicate key value is ({cryptoAutopilotId}).""");
    }
}
