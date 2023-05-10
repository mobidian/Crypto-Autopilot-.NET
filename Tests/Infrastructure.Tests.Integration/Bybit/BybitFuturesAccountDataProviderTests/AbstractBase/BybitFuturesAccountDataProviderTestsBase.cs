using Application.Interfaces.Services.Bybit;

using Bybit.Net.Enums;
using Bybit.Net.Objects.Models;

using Domain.Models;

using Infrastructure.Services.Bybit;
using Infrastructure.Services.General;
using Infrastructure.Tests.Integration.Bybit.Abstract;

namespace Infrastructure.Tests.Integration.Bybit.BybitFuturesAccountDataProviderTests.AbstractBase;

public abstract class BybitFuturesAccountDataProviderTestsBase : BybitServicesTestBase
{
    protected readonly CurrencyPair CurrencyPair = new CurrencyPair("BTC", "USDT");

    protected readonly IBybitFuturesAccountDataProvider SUT;
    protected readonly IBybitUsdFuturesTradingApiClient TradingClient;
    protected readonly IBybitUsdFuturesMarketDataProvider MarketDataProvider;

    public BybitFuturesAccountDataProviderTestsBase() : base()
    {
        this.TradingClient = new BybitUsdFuturesTradingApiClient(this.BybitClient.UsdPerpetualApi.Trading);
        this.SUT = new BybitFuturesAccountDataProvider(this.BybitClient.UsdPerpetualApi.Account);
        this.MarketDataProvider = new BybitUsdFuturesMarketDataProvider(new DateTimeProvider(), this.BybitClient.UsdPerpetualApi.ExchangeData);
    }


    //// //// //// ////


    private readonly List<BybitUsdPerpetualOrder> Orders = new();


    [TearDown]
    public async Task TearDown()
    {
        await Parallel.ForEachAsync(this.Orders, async (order, _) => await this.TradingClient.CloseOrderAsync(order));
        this.Orders.Clear();
    }


    protected async Task<BybitUsdPerpetualOrder> TradingClient_PlaceOrderAsync(string symbol, OrderSide side, OrderType type, decimal quantity, TimeInForce timeInForce, bool reduceOnly, bool closeOnTrigger, decimal? price = null, string? clientOrderId = null, decimal? takeProfitPrice = null, decimal? stopLossPrice = null, TriggerType? takeProfitTriggerType = null, TriggerType? stopLossTriggerType = null, PositionMode? positionMode = null, long? receiveWindow = null)
    {
        var perpetualOrder = await this.TradingClient.PlaceOrderAsync(symbol, side, type, quantity, timeInForce, reduceOnly, closeOnTrigger, price, clientOrderId, takeProfitPrice, stopLossPrice, takeProfitTriggerType, stopLossTriggerType, positionMode, receiveWindow);
        this.Orders.Add(perpetualOrder);
        return perpetualOrder;
    }
}
