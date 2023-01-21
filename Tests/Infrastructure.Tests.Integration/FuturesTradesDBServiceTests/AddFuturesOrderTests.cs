using Application.Mapping;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests;

public class AddFuturesOrderTests : FuturesTradesDBServiceTestsBase
{
    [Test, Order(1)]
    public async Task AddFuturesOrderAsync_ShouldAddFuturesOrder_WhenFuturesOrderIsValidAndCandlestickDoesNotExist()
    {
        // Arrange
        var candlestick = this.CandlestickGenerator.Generate();
        var futuresorder = this.FuturesOrderGenerator.Generate();
        futuresorder.Symbol = candlestick.CurrencyPair.Name;
        
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
        futuresorder.Symbol = candlestick.CurrencyPair.Name;

        await this.SUT.AddCandlestickAsync(candlestick);


        // Act
        await this.SUT.AddFuturesOrderAsync(futuresorder, candlestick);

        // Assert
        var addedEntity = this.dbContext.FuturesOrders.Single();
        base.AssertAgainstAddedEntityAuditRecords(addedEntity);
        addedEntity.ToDomainObject().Should().BeEquivalentTo(futuresorder);
    }

    [Test, Order(3)]
    public async Task AddFuturesOrderAsync_ShouldThrow_WhenFuturesOrderAlreadyExists()
    {
        // Arrange
        var candlestick = this.CandlestickGenerator.Generate();
        var futuresorder = this.FuturesOrderGenerator.Generate();

        // Act
        await this.SUT.AddFuturesOrderAsync(futuresorder, candlestick);
        var func = async () => await this.SUT.AddFuturesOrderAsync(futuresorder, candlestick);
        
        // Assert
        // Assert
        (await func.Should().ThrowExactlyAsync<DbUpdateException>())
            .WithMessage("An error occurred while saving the entity changes. See the inner exception for details.")
            .WithInnerExceptionExactly<SqlException>()
            .WithMessage($"Cannot insert duplicate key row in object 'dbo.FuturesOrders' with unique index 'IX_FuturesOrders_Binance ID'. The duplicate key value is ({futuresorder.Id}).");
    }

    [Test, Order(4)]
    public async Task AddFuturesOrderAsync_ShouldThrow_WhenCandlestickIsNull()
    {
        // Arrange
        var futuresorder = this.FuturesOrderGenerator.Generate();

        // Act
        var func = async () => await this.SUT.AddFuturesOrderAsync(futuresorder, null!);

        // Assert
        (await func.Should().ThrowExactlyAsync<ArgumentNullException>()).WithMessage("Value cannot be null. (Parameter 'Candlestick')");
    }

    [Test, Order(5)]
    public async Task AddFuturesOrderAsync_ShouldThrow_WhenFuturesOrderIsNull()
    {
        // Arrange
        var candlestick = this.CandlestickGenerator.Generate();
        
        // Act
        var func = async () => await this.SUT.AddFuturesOrderAsync(null!, candlestick);

        // Assert
        (await func.Should().ThrowExactlyAsync<ArgumentNullException>()).WithMessage("Value cannot be null. (Parameter 'FuturesOrder')");
    }
}
