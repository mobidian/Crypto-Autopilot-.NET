using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Objects;
using Binance.Net.Objects.Models.Futures;

using Domain.Models;

using Infrastructure.Services.Trading;
using Infrastructure.Tests.Integration.Common;

using NUnit.Framework.Interfaces;

namespace Infrastructure.Tests.Integration.BinanceCfdTradingServiceTests.Base;

public abstract class BinanceCfdTradingServiceTestsBase
{
    private readonly SecretsManager SecretsManager = new SecretsManager();

    protected readonly CurrencyPair CurrencyPair = new CurrencyPair("ETH", "BUSD");

    protected BinanceCfdTradingService SUT = default!;
    protected decimal testMargin = 5;

    protected const decimal precision = 1; // for assertions


    public BinanceCfdTradingServiceTestsBase()
    {
        var binanceClient = new BinanceClient();
        binanceClient.SetApiCredentials(new BinanceApiCredentials(this.SecretsManager.GetSecret("BinanceApiCredentials:key"), this.SecretsManager.GetSecret("BinanceApiCredentials:secret")));

        this.SUT = new BinanceCfdTradingService(this.CurrencyPair, 10, binanceClient, binanceClient.UsdFuturesApi, binanceClient.UsdFuturesApi.Trading, binanceClient.UsdFuturesApi.ExchangeData);
    }


    //// //// //// ////


    private bool StopTests = false; // the test execution stops if this field becomes true

    [SetUp]
    public void SetUp() => Assume.That(this.StopTests, Is.False);

    [TearDown]
    public async Task TearDown()
    {
        this.StopTests = TestContext.CurrentContext.Result.Outcome.Status != TestStatus.Passed;

        for (var i = 0; i < 10 && this.SUT.IsInPosition(); i++)
        {
            try { await this.SUT.ClosePositionAsync(); }
            catch { await Task.Delay(300); }
        }
    }
}
