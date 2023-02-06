using Application.Mapping;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests;

public class AddFuturesOrdersTests : FuturesTradesDBServiceTestsBase
{
    [Test, Order(1)]
    public async Task AddFuturesOrdersAsync_ShouldAddFuturesOrders_WhenFuturesOrdersAreValidAndCandlestickDoesNotExist()
    {
        // Arrange
        var candlestick = this.CandlestickGenerator.Generate();
        var futuresorder = this.FuturesOrderGenerator.Clone().RuleFor(o => o.Symbol, candlestick.CurrencyPair.Name).Generate(this.Random.Next(1, 10));
        
        // Act
        await this.SUT.AddFuturesOrdersAsync(candlestick, futuresorder.ToArray());
        
        // Assert
        var addedEntities = this.DbContext.FuturesOrders.ToList();
        base.AssertAgainstAddedEntitiesAuditRecords(addedEntities);
        addedEntities.Select(x => x.ToDomainObject()).Should().BeEquivalentTo(futuresorder);
    }

    [Test, Order(2)]
    public async Task AddFuturesOrdersAsync_ShouldAddFuturesOrders_WhenFuturesOrdersAreValidAndCandlestickExists()
    {
        // Arrange
        var candlestick = this.CandlestickGenerator.Generate();
        var futuresorder = this.FuturesOrderGenerator.Clone().RuleFor(o => o.Symbol, candlestick.CurrencyPair.Name).Generate(this.Random.Next(1, 10));

        await this.SUT.AddCandlestickAsync(candlestick);


        // Act
        await this.SUT.AddFuturesOrdersAsync(candlestick, futuresorder.ToArray());

        // Assert
        var addedEntities = this.DbContext.FuturesOrders.ToList();
        base.AssertAgainstAddedEntitiesAuditRecords(addedEntities);
        addedEntities.Select(x => x.ToDomainObject()).Should().BeEquivalentTo(futuresorder);
    }
    
    [Test, Order(3)]
    public async Task AddFuturesOrdersAsync_ShouldThrow_WhenFuturesOrderAlreadyExists()
    {
        // Arrange
        var candlestick = this.CandlestickGenerator.Generate();
        var futuresorder = this.FuturesOrderGenerator.Clone().RuleFor(o => o.Symbol, candlestick.CurrencyPair.Name).Generate();

        // Act
        await this.SUT.AddFuturesOrdersAsync(candlestick, futuresorder);
        var func = async () => await this.SUT.AddFuturesOrdersAsync(candlestick, futuresorder);
        
        // Assert
        (await func.Should().ThrowExactlyAsync<DbUpdateException>())
            .WithMessage("An error occurred while saving the entity changes. See the inner exception for details.")
            .WithInnerExceptionExactly<SqlException>()
            .WithMessage($"Cannot insert duplicate key row in object 'dbo.FuturesOrders' with unique index 'IX_FuturesOrders_Binance ID'. The duplicate key value is ({futuresorder.Id}).");
    }

    [Test, Order(4)]
    public async Task AddFuturesOrdersAsync_ShouldThrow_WhenFuturesOrderAndCandlestickDontHaveTheSameCurrencyPair()
    {
        // Arrange
        var candlestick = this.CandlestickGenerator.Generate();
        var futuresorder = this.FuturesOrderGenerator.Clone().RuleFor(o => o.Symbol, f => GetRandomCurrencyPairExcept(f, candlestick.CurrencyPair).Name).Generate();

        // Act
        var func = async () => await this.SUT.AddFuturesOrdersAsync(candlestick, futuresorder);
        
        // Assert
        (await func.Should().ThrowExactlyAsync<DbUpdateException>())
            .WithMessage("An error occurred while saving the entity changes. See the inner exception for details.")
            .WithInnerExceptionExactly<ArgumentException>()
            .WithMessage($"Cannot insert the specified candlestick and futures order since the FuturesOrder.Symbol ({futuresorder.Symbol}) does not match the Candlestick.CurrencyPair ({candlestick.CurrencyPair}). (Parameter 'Symbol')");
    }


    [Test, Order(5)]
    public async Task AddFuturesOrdersAsync_ShouldThrow_WhenCandlestickIsNull()
    {
        // Arrange
        var futuresorder = this.FuturesOrderGenerator.Generate();

        // Act
        var func = async () => await this.SUT.AddFuturesOrdersAsync(null!, futuresorder);

        // Assert
        (await func.Should().ThrowExactlyAsync<ArgumentNullException>()).WithMessage("Value cannot be null. (Parameter 'Candlestick')");
    }

    [Test, Order(6)]
    public async Task AddFuturesOrdersAsync_ShouldThrow_WhenFuturesOrderIsNull()
    {
        // Arrange
        var candlestick = this.CandlestickGenerator.Generate();
        
        // Act
        var func = async () => await this.SUT.AddFuturesOrdersAsync(candlestick, null!);

        // Assert
        (await func.Should().ThrowExactlyAsync<ArgumentNullException>()).WithMessage("Value cannot be null. (Parameter 'FuturesOrders')");
    }
}
