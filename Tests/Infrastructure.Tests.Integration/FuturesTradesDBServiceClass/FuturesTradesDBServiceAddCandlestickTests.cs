using Application.Mapping;

using Domain.Models;

using FluentAssertions;

using Infrastructure.Database.Contexts;
using Infrastructure.Services.Trading;
using Infrastructure.Tests.Integration.FuturesTradesDBServiceClass.Common;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceClass;

public class FuturesTradesDBServiceAddCandlestickTests : FuturesTradesDBServiceTestsBase
{
    [TearDown]
    public async Task TearDown() => await this.ClearDatabaseAsync();
    
    

    [Test]
    public async Task AddCandlestickAsync_ShouldAddCandlestick_WhenCandlestickIsValid()
    {
        // Arrange
        var candlestick = this.CandlestickGenerator.Generate();
        
        // Act
        await this.SUT.AddCandlestickAsync(candlestick);
        
        // Assert
        this.dbContext.Candlesticks.Single().ToDomainObject().Should().BeEquivalentTo(candlestick);
    }

    [Test]
    public async Task AddCandlestickAsync_ShouldThrow_WhenCandlestickIsNull()
    {
        // Act
        var func = async () => await this.SUT.AddCandlestickAsync(null!);

        // Assert
        await func.Should().ThrowAsync<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'Candlestick')");
    }
}
