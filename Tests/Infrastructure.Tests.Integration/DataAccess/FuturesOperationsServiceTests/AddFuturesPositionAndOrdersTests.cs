using Application.Data.Mapping;

using Domain.Models.Futures;

using FluentAssertions;

using Infrastructure.Tests.Integration.Common.DataGenerators;
using Infrastructure.Tests.Integration.Common.Fixtures;
using Infrastructure.Tests.Integration.DataAccess.FuturesOperationsServiceTests.AbstractBase;

using Xunit;

namespace Infrastructure.Tests.Integration.DataAccess.FuturesOperationsServiceTests;

public class AddFuturesPositionAndOrdersTests : FuturesOperationsServiceTestsBase
{
    public AddFuturesPositionAndOrdersTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }


    [Theory]
    [ClassData(typeof(ValidPositionAndOrdersGenerator))]
    public async Task AddFuturesPositionAndOrders_ShouldAddFuturesPositionAndOrders_WhenAllFuturesOrdersRequirePosition(FuturesPosition position, List<FuturesOrder> orders)
    {
        // Act
        await this.SUT.AddFuturesPositionAndOrdersAsync(position, orders);

        // Assert
        this.ArrangeAssertDbContext.FuturesPositions.Single().ToDomainObject().Should().BeEquivalentTo(position);
        this.ArrangeAssertDbContext.FuturesOrders.Select(x => x.ToDomainObject()).Should().BeEquivalentTo(orders);
    }
}
