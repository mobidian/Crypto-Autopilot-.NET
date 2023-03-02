using Application.Interfaces.Services.Trading;
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

namespace Infrastructure.Tests.Integration.BinanceFuturesTradingServiceTestsBase.Base;

public abstract class BinanceFuturesTradingServiceTestsBase
{
    private readonly SecretsManager SecretsManager = new SecretsManager();

    protected const decimal precision = 1; // for assertions
    protected decimal Margin = 5;
    
    protected BinanceFuturesTradingService SUT = default!;
    protected readonly CurrencyPair CurrencyPair = new CurrencyPair("ETH", "BUSD");
    protected readonly decimal Leverage = 10m;
    protected readonly IBinanceClient BinanceClient;
    protected readonly IBinanceFuturesApiService FuturesApiService;
    protected readonly IBinanceClientUsdFuturesApi FuturesClient;
    protected readonly IBinanceClientUsdFuturesApiTrading TradingClient;
    protected readonly IBinanceClientUsdFuturesApiExchangeData FuturesExchangeData;
    protected readonly IBinanceFuturesAccountDataProvider AccountDataProvider;
    protected readonly IFuturesMarketDataProvider MarketDataProvider;
    protected readonly IOrderStatusMonitor OrderStatusMonitor;
    
    public BinanceFuturesTradingServiceTestsBase()
    {
        var apiCredentials = new BinanceApiCredentials(this.SecretsManager.GetSecret("BinanceApiCredentials:key"), this.SecretsManager.GetSecret("BinanceApiCredentials:secret"));
        
        this.BinanceClient = new BinanceClient();
        this.BinanceClient.SetApiCredentials(apiCredentials);

        var binanceSocketClient = new BinanceSocketClient();
        binanceSocketClient.SetApiCredentials(apiCredentials);


        this.FuturesClient = this.BinanceClient.UsdFuturesApi;
        this.TradingClient = this.FuturesClient.Trading;
        this.FuturesExchangeData = this.FuturesClient.ExchangeData;
        this.MarketDataProvider = new BinanceFuturesMarketDataProvider(this.TradingClient, this.FuturesExchangeData);

        var logger = new LoggerAdapter<OrderStatusMonitor>(new Logger<OrderStatusMonitor>(new LoggerFactory()));
        this.OrderStatusMonitor = new OrderStatusMonitor(this.FuturesClient.Account, binanceSocketClient.UsdFuturesStreams, new UpdateSubscriptionProxy(), logger);

        this.FuturesApiService = new BinanceFuturesApiService(this.TradingClient, this.MarketDataProvider, this.OrderStatusMonitor);

        this.AccountDataProvider = new BinanceFuturesAccountDataProvider(this.FuturesClient.Account);

        this.SUT = new BinanceFuturesTradingService(this.CurrencyPair, 10, this.FuturesApiService, this.AccountDataProvider, this.MarketDataProvider);
    }


    //// //// //// ////
    
    private readonly List<long> LimitOrdersIDs = new List<long>();
    private bool StopTests = false; // the test execution stops if this field becomes true

    [SetUp]
    public void SetUp() => Assume.That(this.StopTests, Is.False);

    [TearDown]
    public async Task TearDown()
    {
        this.StopTests = TestContext.CurrentContext.Result.Outcome.Status != TestStatus.Passed;

        
        if (this.SUT.IsInPosition())
            await this.SUT.ClosePositionAsync();

        await this.TradingClient.CancelMultipleOrdersAsync(this.CurrencyPair.Name, this.LimitOrdersIDs);
        this.LimitOrdersIDs.Clear();
    }


    protected async Task<BinanceFuturesOrder> SUT_PlaceLimitOrderAsync(OrderSide OrderSide, decimal LimitPrice, decimal QuoteMargin = decimal.MaxValue, decimal? StopLoss = null, decimal? TakeProfit = null)
    {
        var task = this.SUT.PlaceLimitOrderAsync(OrderSide, LimitPrice, this.Margin, StopLoss, TakeProfit);
        
        var order = await task;
        this.LimitOrdersIDs.Add(order.Id);
         
        return task.Result;
    }
}
