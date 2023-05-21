using Domain.Models.Signals;

using Infrastructure.Tests.Integration.DataAccess.TradingSignalsRepositoryTests.AbstractBase;

namespace Infrastructure.Tests.Integration.DataAccess.TradingSignalsRepositoryTests;

public class GetAllTests : TradingSignalsRepositoryTestsBase
{
    [Test]
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
    
    [Test]
    public async Task GetAll_ShouldReturnEmptyEnumerable_WhenTradingNoSignalsExist()
    {
        // Act
        var retrievedTradingSignals = await this.SUT.GetAllAsync();

        // Assert
        retrievedTradingSignals.Should().BeEmpty();
    }
}
