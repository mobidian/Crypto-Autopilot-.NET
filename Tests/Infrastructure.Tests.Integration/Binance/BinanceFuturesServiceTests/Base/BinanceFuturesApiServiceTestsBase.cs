using Application.Interfaces.Services.Trading.Binance;
using Application.Interfaces.Services.Trading.Binance.Monitors;

using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Interfaces.Clients.UsdFuturesApi;
using Binance.Net.Objects;
using Binance.Net.Objects.Models.Futures;

using Domain.Extensions;
using Domain.Models;

using Infrastructure.Logging;
using Infrastructure.Services.Proxies;
using Infrastructure.Services.Trading;
using Infrastructure.Services.Trading.Binance;
using Infrastructure.Services.Trading.Binance.Monitors;
using Infrastructure.Tests.Integration.Common;

using Microsoft.Extensions.Logging;

using NUnit.Framework.Interfaces;

namespace Infrastructure.Tests.Integration.Binance.BinanceFuturesServiceTests.Base;

public abstract class BinanceFuturesApiServiceTestsBaseClass
{
    private readonly SecretsManager SecretsManager = new SecretsManager();

    protected const decimal precision = 1; // for assertions
    protected decimal Margin = 5;

    protected readonly BinanceFuturesApiService SUT;

    protected readonly CurrencyPair CurrencyPair = new CurrencyPair("ETH", "BUSD");
    protected readonly decimal Leverage = 10m;

    private readonly IBinanceClientUsdFuturesApiTrading TradingClient;
    protected readonly IFuturesMarketDataProvider MarketDataProvider;
    private readonly IOrderStatusMonitor OrderStatusMonitor;

    public BinanceFuturesApiServiceTestsBaseClass()
    {
        var apiCredentials = new BinanceApiCredentials(this.SecretsManager.GetSecret("BinanceApiCredentials:key"), this.SecretsManager.GetSecret("BinanceApiCredentials:secret"));

        var binanceClient = new BinanceClient();
        binanceClient.SetApiCredentials(apiCredentials);

        var binanceSocketClient = new BinanceSocketClient();
        binanceSocketClient.SetApiCredentials(apiCredentials);

        this.TradingClient = binanceClient.UsdFuturesApi.Trading;
        this.MarketDataProvider = new BinanceFuturesMarketDataProvider(this.TradingClient, binanceClient.UsdFuturesApi.ExchangeData);
        this.OrderStatusMonitor = new OrderStatusMonitor(binanceClient.UsdFuturesApi.Account, binanceSocketClient.UsdFuturesStreams, new UpdateSubscriptionProxy(), new LoggerAdapter<OrderStatusMonitor>(new Logger<OrderStatusMonitor>(new LoggerFactory())));


        this.SUT = new BinanceFuturesApiService(this.TradingClient, this.MarketDataProvider, this.OrderStatusMonitor);
    }


    //// //// //// ////

    private readonly List<long> LimitOrdersIDs = new List<long>();
    private readonly List<BinanceFuturesOrder> MarketOrders = new List<BinanceFuturesOrder>();

    private bool StopTests = false; // the test execution stops if this field becomes true

    [SetUp]
    public void SetUp() => Assume.That(this.StopTests, Is.False);

    [TearDown]
    public async Task TearDown()
    {
        this.StopTests = TestContext.CurrentContext.Result.Outcome.Status != TestStatus.Passed;


        foreach (var order in this.MarketOrders)
            await this.TradingClient.PlaceOrderAsync(this.CurrencyPair.Name, order.Side.Invert(), FuturesOrderType.Market, order.Quantity, positionSide: order.PositionSide);

        await this.TradingClient.CancelMultipleOrdersAsync(this.CurrencyPair.Name, this.LimitOrdersIDs);
        this.LimitOrdersIDs.Clear();
    }


    protected async Task<IEnumerable<BinanceFuturesOrder>> SUT_PlaceMarketOrderAsync(string currencyPair, OrderSide orderSide, decimal Margin, decimal Leverage, decimal? StopLoss = null, decimal? TakeProfit = null)
    {
        var orders = await this.SUT.PlaceMarketOrderAsync(currencyPair, orderSide, Margin, Leverage, StopLoss, TakeProfit);
        var ordersArray = orders.ToArray();

        this.MarketOrders.Add(ordersArray[0]);

        if (ordersArray.Length > 1)
            this.LimitOrdersIDs.Add(ordersArray[1].Id);

        if (ordersArray.Length > 2)
            this.LimitOrdersIDs.Add(ordersArray[2].Id);

        return orders;
    }
}
