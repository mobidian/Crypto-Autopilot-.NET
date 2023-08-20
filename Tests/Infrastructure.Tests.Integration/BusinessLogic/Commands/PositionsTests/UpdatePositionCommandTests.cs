using Application.Data.Mapping;
using Application.Extensions.Bybit;

using Bybit.Net.Enums;

using Domain.Commands.Positions;
using Domain.Models.Futures;

using FluentAssertions;

using Infrastructure.Tests.Integration.BusinessLogic.Commands.AbstractBase;

using Tests.Integration.Common.DataAccess.Extensions;
using Tests.Integration.Common.DataGenerators;
using Tests.Integration.Common.Fixtures;

using Xunit;

namespace Infrastructure.Tests.Integration.BusinessLogic.Commands.PositionsTests;

public class UpdatePositionCommandTests : CommandsTestsBase
{
    public UpdatePositionCommandTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }


    [Theory]
    [ClassData(typeof(ValidPositionAndOrdersGenerator))]
    public async Task UpdatePositionCommand_ShouldUpdateFuturesPosition_WhenCommandIsValid(FuturesPosition position, List<FuturesOrder> orders)
    {
        // Arrange
        await this.Mediator.Send(new CreatePositionCommand
        {
            Position = position,
            FuturesOrders = orders,
        });

        var updatedPosition = this.FuturesPositionsGenerator.Clone()
            .RuleFor(x => x.CryptoAutopilotId, position.CryptoAutopilotId)
            .RuleFor(x => x.Side, position.Side)
            .Generate();

        var command = new UpdatePositionCommand
        {
            UpdatedPosition = updatedPosition
        };


        // Act
        await this.Mediator.Send(command);


        // Assert
        this.ArrangeAssertDbContext.FuturesOrders.Select(x => x.ToDomainObject()).Should().BeEquivalentTo(orders);
        this.ArrangeAssertDbContext.FuturesPositions.Single().ToDomainObject().Should().BeEquivalentTo(updatedPosition);
    }

    [Theory]
    [ClassData(typeof(ValidPositionAndOrdersGenerator))]
    public async Task UpdatePositionCommand_ShouldUpdateFuturesPositionAndAddOrders_WhenCommandOrdersAreSpecified(FuturesPosition position, List<FuturesOrder> orders)
    {
        // Arrange
        await this.Mediator.Send(new CreatePositionCommand
        {
            Position = position,
            FuturesOrders = orders,
        });

        var newFuturesOrders = this.FuturesOrdersGenerator.Generate(3, $"default, {OrderType.Market.ToRuleSetName()}, {position.Side.ToRuleSetName()}");
        var updatedPosition = this.FuturesPositionsGenerator.Clone()
            .RuleFor(x => x.CryptoAutopilotId, position.CryptoAutopilotId)
            .RuleFor(x => x.Side, position.Side)
            .Generate();

        var command = new UpdatePositionCommand
        {
            UpdatedPosition = updatedPosition,
            NewFuturesOrders = newFuturesOrders,
        };


        // Act
        await this.Mediator.Send(command);


        // Assert
        this.ArrangeAssertDbContext.FuturesOrders.Select(x => x.ToDomainObject()).Should().BeEquivalentTo(orders.Concat(newFuturesOrders));
        this.ArrangeAssertDbContext.FuturesPositions.Single().ToDomainObject().Should().BeEquivalentTo(updatedPosition);
    }

    [Theory]
    [ClassData(typeof(ValidPositionAndOrdersGenerator))]
    public async Task UpdatePositionCommand_ShouldThrow_WhenThePositionSideOfAnyOrderDoesNotMatchTheSideOfThePosition(FuturesPosition position, List<FuturesOrder> orders)
    {
        // Arrange
        await this.Mediator.Send(new CreatePositionCommand
        {
            Position = position,
            FuturesOrders = orders,
        });

        var invertedSideRule = position.Side.Invert().ToRuleSetName();
        var orderOpposidePositionSide = this.FuturesOrdersGenerator.Generate($"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}, {invertedSideRule}");
        var newFuturesOrders = this.FuturesOrdersGenerator.Generate(3, $"default, {OrderType.Market.ToRuleSetName()}, {position.Side.ToRuleSetName()}");
        newFuturesOrders[Random.Shared.Next(orders.Count)] = orderOpposidePositionSide;

        var updatedPosition = this.FuturesPositionsGenerator.Clone()
            .RuleFor(x => x.CryptoAutopilotId, position.CryptoAutopilotId)
            .RuleFor(x => x.Side, position.Side)
            .Generate();

        var command = new UpdatePositionCommand
        {
            UpdatedPosition = updatedPosition,
            NewFuturesOrders = newFuturesOrders,
        };


        // Act
        var func = async () => await this.Mediator.Send(command);


        // Assert
        (await func.Should()
            .ThrowExactlyAsync<FluentValidation.ValidationException>())
            .And.Errors.Should().ContainSingle(x => x.ErrorMessage == "The position side must match the position side of the related orders.");
    }
}
