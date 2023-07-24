using Domain.Models.Signals;

using FluentAssertions;

using Infrastructure.Tests.Integration.DataAccess.TradingSignalsRepositoryTests.AbstractBase;

using Tests.Integration.Common.Fixtures;

using Xunit;

namespace Infrastructure.Tests.Integration.DataAccess.TradingSignalsRepositoryTests;

public class GetAllTests : TradingSignalsRepositoryTestsBase
{
    public GetAllTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }


    [Fact]
    public async Task GetAll_ShouldReturnAllTradingSignals_WhenTradingSignalsExist()
    {
        // Arrange
        var tradingSignals = this.TradingSignalGenerator.Generate(100);
        await this.SUT.AddAsync(tradingSignals);
        
        // Act
        var retrievedTradingSignals = await this.SUT.GetAllAsync();

        // Assert
        retrievedTradingSignals.Should().BeEquivalentTo(tradingSignals);
    }
    
    [Fact]
    public async Task GetAll_ShouldReturnEmptyEnumerable_WhenTradingNoSignalsExist()
    {
        // Act
        var retrievedTradingSignals = await this.SUT.GetAllAsync();

        // Assert
        retrievedTradingSignals.Should().BeEmpty();
    }
}
