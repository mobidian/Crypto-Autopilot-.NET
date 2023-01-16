using CryptoExchange.Net.Authentication;

using Domain.Models;

using Infrastructure.Services.Trading;

using NUnit.Framework.Interfaces;

namespace Infrastructure.Tests.Integration.BinanceCfdTradingServiceTests.Common;

public abstract class BinanceCfdTradingServiceTestsBase
{
    protected readonly ApiCredentials BinanceApiCredentials = Credentials.BinanceIntegrationTestingAPICredentials;
    protected readonly CurrencyPair CurrencyPair = new CurrencyPair("ETH", "BUSD");

    protected BinanceCfdTradingService SUT = default!;
    protected decimal testMargin = 5;



    [OneTimeSetUp]
    public virtual void OneTimeSetUp() => this.SUT = new BinanceCfdTradingService(this.CurrencyPair, this.BinanceApiCredentials);



    
    private bool StopTests = false; // the test execution stops if this field becomes true

    [SetUp]
    public void SetUp() => Assume.That(this.StopTests, Is.False);

    [TearDown]
    public async Task TearDown()
    {
        this.StopTests = TestContext.CurrentContext.Result.Outcome.Status != TestStatus.Passed;
        
        for (int i = 0; i < 10 && this.SUT.IsInPosition(); i++)
        {
            try { await this.SUT.ClosePositionAsync(); }
            catch { await Task.Delay(300); }
        }
    }
}
