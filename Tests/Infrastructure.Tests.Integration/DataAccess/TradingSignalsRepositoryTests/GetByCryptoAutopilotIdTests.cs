using FluentAssertions;

using Infrastructure.Tests.Integration.DataAccess.TradingSignalsRepositoryTests.AbstractBase;

using Tests.Integration.Common.Fixtures;

using Xunit;

namespace Infrastructure.Tests.Integration.DataAccess.TradingSignalsRepositoryTests;

public class GetByCryptoAutopilotIdTests : TradingSignalsRepositoryTestsBase
{
    public GetByCryptoAutopilotIdTests(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }


    [Fact]
    public async Task GetByCryptoAutopilotId_ShouldReturnTradingSignalWithMatchingCryptoAutopilotId_WhenItExists()
    {
        // Arrange
        var tradingSignals = this.TradingSignalGenerator.Generate(10);
        await this.SUT.AddAsync(tradingSignals);

        var tradingSignal = tradingSignals[Random.Shared.Next(tradingSignals.Count)];


        // Act
        var retrievedTradingSignal = await this.SUT.GetByCryptoAutopilotIdAsync(tradingSignal.CryptoAutopilotId);
        
        // Assert
        retrievedTradingSignal.Should().BeEquivalentTo(tradingSignal);
    }
    
    [Fact]
    public async Task GetByCryptoAutopilotId_ShouldReturnNull_WhenTradingSignalWithMatchingCryptoAutopilotIdDoesNotExist()
    {
        // Arrange
        var guid = Guid.NewGuid();
        
        // Act
        var retrievedTradingSignal = await this.SUT.GetByCryptoAutopilotIdAsync(guid);

        // Assert
        retrievedTradingSignal.Should().BeNull();
    }
}
