using Application.Mapping;

using FluentAssertions;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceClass.Common;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceClass;

public class FuturesTradesDBServiceAddFuturesOrderTests : FuturesTradesDBServiceTestsBase
{
    [TearDown]
    public async Task TearDown() => await this.ClearDatabaseAsync();


    [Test, Order(1)]
    public async Task AddFuturesOrderAsync_ShouldAddFuturesOrder_WhenFuturesOrderIsValidAndCandlestickDoesNotExist()
    {
        // Arrange
        var candlestick = this.CandlestickGenerator.Generate();
        var futuresorder = this.FuturesOrderGenerator.Generate();
        
        // Act
        await this.SUT.AddFuturesOrderAsync(futuresorder, candlestick);

        // Assert
        var addedEntity = this.dbContext.FuturesOrders.Single();
        base.AssertAgainstAddedEntityAuditRecords(addedEntity);
        addedEntity.ToDomainObject().Should().BeEquivalentTo(futuresorder);
    }

    [Test, Order(2)]
    public async Task AddFuturesOrderAsync_ShouldAddFuturesOrder_WhenFuturesOrderIsValidAndCandlestickDoesExists()
    {
        // Arrange
        var candlestick = this.CandlestickGenerator.Generate();
        var futuresorder = this.FuturesOrderGenerator.Generate();

        await this.SUT.AddCandlestickAsync(candlestick);


        // Act
        await this.SUT.AddFuturesOrderAsync(futuresorder, candlestick);

        // Assert
        var addedEntity = this.dbContext.FuturesOrders.Single();
        base.AssertAgainstAddedEntityAuditRecords(addedEntity);
        addedEntity.ToDomainObject().Should().BeEquivalentTo(futuresorder);
    }
}
