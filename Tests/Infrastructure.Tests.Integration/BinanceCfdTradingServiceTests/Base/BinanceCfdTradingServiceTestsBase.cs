using Application.Interfaces.Services.Trading.Monitors;

using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Interfaces.Clients;
using Binance.Net.Interfaces.Clients.UsdFuturesApi;
using Binance.Net.Objects;
using Binance.Net.Objects.Models.Futures;

using Domain.Models;

using Infrastructure.Logging;
using Infrastructure.Services.Proxies;
using Infrastructure.Services.Trading;
using Infrastructure.Services.Trading.Monitors;
using Infrastructure.Tests.Integration.Common;

using Microsoft.Extensions.Logging;

using NUnit.Framework.Interfaces;

namespace Infrastructure.Tests.Integration.BinanceCfdTradingServiceTests.Base;

public abstract class BinanceCfdTradingServiceTestsBase
{
    private readonly SecretsManager SecretsManager = new SecretsManager();

    protected const decimal precision = 1; // for assertions
    protected decimal testMargin = 5;

    protected BinanceCfdTradingService SUT = default!;
    protected readonly CurrencyPair CurrencyPair = new CurrencyPair("ETH", "BUSD");
    protected readonly decimal Leverage = 10m;
    private readonly IBinanceClient BinanceClient;
    private readonly IBinanceClientUsdFuturesApi FuturesClient;
    private readonly IBinanceClientUsdFuturesApiTrading TradingClient;
    private readonly IBinanceClientUsdFuturesApiExchangeData FuturesExchangeData;
    private readonly IOrderStatusMonitor OrderStatusMonitor;

    public BinanceCfdTradingServiceTestsBase()
    {
        var apiCredentials = new BinanceApiCredentials(this.SecretsManager.GetSecret("BinanceApiCredentials:key"), this.SecretsManager.GetSecret("BinanceApiCredentials:secret"));

        this.BinanceClient = new BinanceClient();
        this.BinanceClient.SetApiCredentials(apiCredentials);

        var binanceSocketClient = new BinanceSocketClient();
        binanceSocketClient.SetApiCredentials(apiCredentials);


        this.FuturesClient = this.BinanceClient.UsdFuturesApi;
        this.TradingClient = this.FuturesClient.Trading;
        this.FuturesExchangeData = this.FuturesClient.ExchangeData;

        var logger = new LoggerAdapter<OrderStatusMonitor>(new Logger<OrderStatusMonitor>(new LoggerFactory()));
        this.OrderStatusMonitor = new OrderStatusMonitor(this.FuturesClient.Account, binanceSocketClient.UsdFuturesStreams, new UpdateSubscriptionProxy(), logger);


        this.SUT = new BinanceCfdTradingService(this.CurrencyPair, 10, this.BinanceClient, this.FuturesClient, this.TradingClient, this.FuturesExchangeData, this.OrderStatusMonitor);
    }


    //// //// //// ////
    
    private readonly List<long> LimitOrdersIDs;
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



    protected async Task<BinanceFuturesPlacedOrder> SUT_PlaceLimitOrderAsync(OrderSide OrderSide, decimal LimitPrice, decimal QuoteMargin = decimal.MaxValue, decimal? StopLoss = null, decimal? TakeProfit = null)
    {
        var task = this.SUT.PlaceLimitOrderAsync(OrderSide.Buy, LimitPrice, this.testMargin, StopLoss, TakeProfit);

        var callResult = await task;
        this.LimitOrdersIDs.Add(callResult.Id);
         
        return task.Result;
    }
}
