using Application.Mapping;

using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests;

public class AddCandlestickTests : FuturesTradesDBServiceTestsBase
{
    [Test, Order(1)]
    public async Task AddCandlestickAsync_ShouldAddCandlestick_WhenCandlestickIsValid()
    {
        // Arrange
        var candlestick = this.CandlestickGenerator.Generate();

        // Act
        await this.SUT.AddCandlestickAsync(candlestick);

        // Assert
        var addedEntity = this.DbContext.Candlesticks.Single();
        AssertAgainstAddedEntitiesAuditRecords(new[] { addedEntity });
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
            .WithMessage($"Cannot insert duplicate key row in object 'dbo.Candlesticks' with unique index 'IX_Candlesticks_Currency Pair_DateTime'. The duplicate key value is ({candlestick.CurrencyPair}, {candlestick.Date:yyyy-MM-dd HH:mm:ss.fffffff}).");
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
