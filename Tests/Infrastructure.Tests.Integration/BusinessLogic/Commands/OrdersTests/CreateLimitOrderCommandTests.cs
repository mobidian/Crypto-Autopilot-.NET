using Application.Data.Mapping;

using Bybit.Net.Enums;

using Domain.Commands.Orders;

using FluentAssertions;

using Infrastructure.Tests.Integration.BusinessLogic.Commands.AbstractBase;
using Infrastructure.Tests.Integration.Common.Fixtures;
using Infrastructure.Tests.Integration.DataAccess.Extensions;

using Xunit;

namespace Infrastructure.Tests.Integration.BusinessLogic.Commands.OrdersTests;

public class CreateLimitOrderCommandTests : CommandsTestsBase
{
    public CreateLimitOrderCommandTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    
    [Fact]
    public async Task CreateLimitOrderCommand_ShouldCreateLimitOrder_WhenCommandIsValid()
    {
        // Arrange
        var limitOrder = this.FuturesOrdersGenerator.Generate($"default, {OrderType.Limit.ToRuleSetName()}");
        var command = new CreateLimitOrderCommand
        {
            LimitOrder = limitOrder,
        };

        // Act
        await this.Mediator.Send(command);
        
        // Assert
        this.ArrangeAssertDbContext.FuturesOrders.Single().ToDomainObject().Should().BeEquivalentTo(limitOrder);
    }

    [Theory]
    [MemberData(nameof(GetPositionOpeningOrders))]
    public async Task CreateLimitOrderCommand_ShouldThrow_WhenOrderOpensPosition(string ruleSet, string expectedErrorMessage)
    {
        // Arrange
        var order = this.FuturesOrdersGenerator.Generate($"default, {ruleSet}");
        var command = new CreateLimitOrderCommand
        {
            LimitOrder = order,
        };
        
        // Act
        var func = async () => await this.Mediator.Send(command);
        
        // Assert
        (await func.Should()
            .ThrowExactlyAsync<FluentValidation.ValidationException>())
            .And.Errors.Should().ContainSingle(x => x.ErrorMessage == expectedErrorMessage);
    }
    public static IEnumerable<object[]> GetPositionOpeningOrders()
    {
        yield return new object[] { $"{OrderType.Market.ToRuleSetName()}", "'Limit Order Type' must be equal to 'Limit'." };
        yield return new object[] { $"{OrderType.Limit.ToRuleSetName()}, {OrderStatus.Filled.ToRuleSetName()}", "'Limit Order Status' must not be equal to 'Filled'." };
    }
}
