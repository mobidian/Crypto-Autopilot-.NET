using Application.Data.Mapping;
using Application.Extensions.Bybit;

using Bybit.Net.Enums;

using Domain.Commands.Positions;
using Domain.Models.Futures;

using FluentAssertions;

using Infrastructure.Tests.Integration.BusinessLogic.Commands.AbstractBase;
using Infrastructure.Tests.Integration.Common.DataGenerators;
using Infrastructure.Tests.Integration.Common.Fixtures;

using Tests.Integration.Common.DataAccess.Extensions;

using Xunit;

namespace Infrastructure.Tests.Integration.BusinessLogic.Commands.PositionsTests;

public class CreatePositionCommandTests : CommandsTestsBase
{
    public CreatePositionCommandTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }
    
    
    [Theory]
    [ClassData(typeof(ValidPositionAndOrdersGenerator))]
    public async Task CreatePositionCommand_ShouldCreateFuturesPositionAndOrders_WhenCommandIsValid(FuturesPosition position, List<FuturesOrder> orders)
    {
        // Arrange
        var command = new CreatePositionCommand
        {
            Position = position,
            FuturesOrders = orders,
        };

        // Act
        await this.Mediator.Send(command);

        // Assert
        this.ArrangeAssertDbContext.FuturesOrders.Select(x => x.ToDomainObject()).Should().BeEquivalentTo(command.FuturesOrders);
        this.ArrangeAssertDbContext.FuturesPositions.Single().ToDomainObject().Should().BeEquivalentTo(command.Position);
    }

    [Theory]
    [ClassData(typeof(ValidPositionAndOrdersGenerator))]
    public async Task CreatePositionCommand_ShouldThrow_WhenAnyFuturesOrderDoesNotRequirePosition(FuturesPosition position, List<FuturesOrder> orders)
    {
        // Arrange
        var limitOrderNotFilled = this.FuturesOrdersGenerator.Generate($"default, {OrderType.Limit.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}");
        orders[Random.Shared.Next(orders.Count)] = limitOrderNotFilled;

        var command = new CreatePositionCommand
        {
            FuturesOrders = orders,
            Position = position,
        };


        // Act
        var func = async () => await this.Mediator.Send(command);


        // Assert
        (await func.Should()
            .ThrowExactlyAsync<FluentValidation.ValidationException>())
            .And.Errors.Should().ContainSingle(x => x.ErrorMessage == "A limit order which has not been filled must not point to a position.");
    }

    [Theory]
    [ClassData(typeof(ValidPositionAndOrdersGenerator))]
    public async Task CreatePositionCommand_ShouldThrow_WhenThePositionSideOfAnyOrderDoesNotMatchTheSideOfThePosition(FuturesPosition position, List<FuturesOrder> orders)
    {
        // Arrange
        var invertedSideRule = position.Side.Invert().ToRuleSetName();
        var orderOpposidePositionSide = this.FuturesOrdersGenerator.Generate($"default, {OrderType.Market.ToRuleSetName()}, {OrderSide.Buy.ToRuleSetName()}, {invertedSideRule}");
        orders[Random.Shared.Next(orders.Count)] = orderOpposidePositionSide;
        
        var command = new CreatePositionCommand
        {
            FuturesOrders = orders,
            Position = position,
        };


        // Act
        var func = async () => await this.Mediator.Send(command);


        // Assert
        (await func.Should()
            .ThrowExactlyAsync<FluentValidation.ValidationException>())
            .And.Errors.Should().ContainSingle(x => x.ErrorMessage == "The position side must match the position side of the related orders.");
    }
}
