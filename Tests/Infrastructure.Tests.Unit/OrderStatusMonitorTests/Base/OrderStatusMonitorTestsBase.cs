using System.Net;

using Application.Interfaces.Logging;
using Application.Interfaces.Proxies;
using Application.Interfaces.Services.Trading;

using Binance.Net.Enums;
using Binance.Net.Interfaces.Clients.UsdFuturesApi;
using Binance.Net.Objects.Models;
using Binance.Net.Objects.Models.Futures.Socket;

using CryptoExchange.Net.Objects;

using CryptoExchange.Net.Sockets;

using Infrastructure.Services.Trading;

namespace Infrastructure.Tests.Unit.OrderStatusMonitorTests.Base;

public abstract class OrderStatusMonitorTestsBase
{
    protected readonly OrderStatusMonitor SUT;
    
    protected readonly string listenKey = "accountlistenKey";
    protected readonly IBinanceClientUsdFuturesApiAccount Account = Substitute.For<IBinanceClientUsdFuturesApiAccount>();
    protected readonly IBinanceSocketClientUsdFuturesStreams FuturesStreams = Substitute.For<IBinanceSocketClientUsdFuturesStreams>();
    protected readonly IUpdateSubscriptionProxy UserDataUpdatesSubscription = Substitute.For<IUpdateSubscriptionProxy>();
    protected readonly ILoggerAdapter<OrderStatusMonitor> Logger = Substitute.For<ILoggerAdapter<OrderStatusMonitor>>();
    protected readonly IDictionary<long, OrderStatus?> OrdersStatuses = new Dictionary<long, OrderStatus?>();

    public OrderStatusMonitorTestsBase()
    {
        var listenKeyWebCallReuslt = new WebCallResult<string>(HttpStatusCode.OK, null, TimeSpan.Zero, this.listenKey, String.Empty, String.Empty, HttpMethod.Get, null, this.listenKey, null);
        this.Account.StartUserStreamAsync().Returns(Task.FromResult(listenKeyWebCallReuslt));

        var updateSubscriptionCallResult = new CallResult<UpdateSubscription>(new UpdateSubscription(null!, null!));
        this.FuturesStreams
            .SubscribeToUserDataUpdatesAsync(
                listenKey: Arg.Is<string>(this.listenKey),
                onLeverageUpdate: Arg.Any<Action<DataEvent<BinanceFuturesStreamConfigUpdate>>>(),
                onMarginUpdate: Arg.Any<Action<DataEvent<BinanceFuturesStreamMarginUpdate>>>(),
                onAccountUpdate: Arg.Any<Action<DataEvent<BinanceFuturesStreamAccountUpdate>>>(),
                onOrderUpdate: Arg.Any<Action<DataEvent<BinanceFuturesStreamOrderUpdate>>>(),
                onListenKeyExpired: Arg.Any<Action<DataEvent<BinanceStreamEvent>>>(),
                onStrategyUpdate: Arg.Any<Action<DataEvent<BinanceStrategyUpdate>>>(),
                onGridUpdate: Arg.Any<Action<DataEvent<BinanceGridUpdate>>>())
            .Returns(Task.FromResult(updateSubscriptionCallResult));

        this.SUT = new OrderStatusMonitor(this.Account, this.FuturesStreams, this.UserDataUpdatesSubscription, this.Logger, this.OrdersStatuses);
    }


    
    protected DataEvent<BinanceFuturesStreamOrderUpdate> CreateDataEvent(long OrderId, OrderStatus status)
    {
        var orderUpdateData = new BinanceFuturesStreamOrderUpdateData();
        orderUpdateData.OrderId = OrderId;
        orderUpdateData.Status = status;

        var orderUpdate = new BinanceFuturesStreamOrderUpdate();
        orderUpdate.ListenKey = this.listenKey;
        orderUpdate.UpdateData = orderUpdateData;

        return new DataEvent<BinanceFuturesStreamOrderUpdate>(orderUpdate, DateTime.MinValue);
    }
}
