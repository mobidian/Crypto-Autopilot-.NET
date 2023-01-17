using Application.Mapping;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests;

public class FuturesTradesDBServiceAddCandlestickTests : FuturesTradesDBServiceTestsBase
{
    [TearDown]
    public async Task TearDown() => await this.ClearDatabaseAsync();
    
    

    [Test, Order(1)]
    public async Task AddCandlestickAsync_ShouldAddCandlestick_WhenCandlestickIsValid()
    {
        // Arrange
        var candlestick = this.CandlestickGenerator.Generate();
        
        // Act
        await this.SUT.AddCandlestickAsync(candlestick);

        // Assert
        var addedEntity = this.dbContext.Candlesticks.Single();
        base.AssertAgainstAddedEntityAuditRecords(addedEntity);
        addedEntity.ToDomainObject().Should().BeEquivalentTo(candlestick);
    }

    [Test, Order(2)]
    public async Task AddCandlestickAsync_ShouldThrow_WhenCandlestickAlreadyExists()
    {
        // Arrange
        var candlestick = this.CandlestickGenerator.Generate();
        var cloneCandlestick = this.CandlestickGenerator.Clone()
            .RuleFor(c => c.CurrencyPair, f => candlestick.CurrencyPair)
            .RuleFor(c => c.Date, f => candlestick.Date)
            .Generate();

        // Act
        await this.SUT.AddCandlestickAsync(candlestick);
        var func = async () => await this.SUT.AddCandlestickAsync(cloneCandlestick);
        
        // Assert
        (await func.Should().ThrowExactlyAsync<DbUpdateException>())
            .WithMessage("An error occurred while saving the entity changes. See the inner exception for details.")
            .WithInnerExceptionExactly<SqlException>()
            .WithMessage($"Cannot insert duplicate key row in object 'dbo.Candlesticks' with unique index 'IX_Candlesticks_Base Currency_Quote Currency_DateTime'. The duplicate key value is ({candlestick.CurrencyPair.Base}, {candlestick.CurrencyPair.Quote}, {candlestick.Date:yyyy-MM-dd HH:mm:ss.fffffff}).");
    }

    [Test, Order(3)]
    public async Task AddCandlestickAsync_ShouldThrow_WhenCandlestickIsNull()
    {
        // Act
        var func = async () => await this.SUT.AddCandlestickAsync(null!);

        // Assert
        (await func.Should().ThrowExactlyAsync<ArgumentNullException>()).WithMessage("Value cannot be null. (Parameter 'Candlestick')");
    }
}
