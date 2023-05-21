using Domain.Models.Signals;

using Infrastructure.Tests.Integration.DataAccess.TradingSignalsRepositoryTests.AbstractBase;

namespace Infrastructure.Tests.Integration.DataAccess.TradingSignalsRepositoryTests;

public class GetAllWithContractTests : TradingSignalsRepositoryTestsBase
{
    [Test]
    public async Task GetAllWithContract_ShouldReturnTradingSignalsWithMatchingContract_WhenTheyExist()
    {
        // Arrange
        var btcusdtSignals = this.TradingSignalGenerator.Clone().RuleFor(x => x.CurrencyPair, "BTCUSDT").Generate(Random.Shared.Next(10, 100));
        var ethusdtSignals = this.TradingSignalGenerator.Clone().RuleFor(x => x.CurrencyPair, "ETHUSDT").Generate(Random.Shared.Next(10, 100));
        var bnbusdtSignals = this.TradingSignalGenerator.Clone().RuleFor(x => x.CurrencyPair, "BNBUSDT").Generate(Random.Shared.Next(10, 100));
        
        var list = new List<TradingSignal>();
        list.AddRange(btcusdtSignals);
        list.AddRange(ethusdtSignals);
        list.AddRange(bnbusdtSignals);

        await this.SUT.AddAsync(list);


        // Act
        var retrievedTradingSignals = await this.SUT.GetAllWithContractAsync("BTCUSDT");


        // Assert
        retrievedTradingSignals.Should().BeEquivalentTo(btcusdtSignals);
    }
    
    [Test]
    public async Task GetAllWithContract_ShouldReturnEmptyEnumerable_WhenTradingSignalsWithMatchingContractDoNotExist()
    {
        // Arrange
        var btcusdtSignals = this.TradingSignalGenerator.Clone().RuleFor(x => x.CurrencyPair, "BTCUSDT").Generate(Random.Shared.Next(10, 100));
        var ethusdtSignals = this.TradingSignalGenerator.Clone().RuleFor(x => x.CurrencyPair, "ETHUSDT").Generate(Random.Shared.Next(10, 100));
        var bnbusdtSignals = this.TradingSignalGenerator.Clone().RuleFor(x => x.CurrencyPair, "BNBUSDT").Generate(Random.Shared.Next(10, 100));

        var list = new List<TradingSignal>();
        list.AddRange(btcusdtSignals);
        list.AddRange(ethusdtSignals);
        list.AddRange(bnbusdtSignals);

        await this.SUT.AddAsync(list);

        
        // Act
        var retrievedTradingSignals = await this.SUT.GetAllWithContractAsync("EURUSD");


        // Assert
        retrievedTradingSignals.Should().BeEmpty();
    }
}
