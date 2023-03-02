using Application.Interfaces.Logging;
using Application.Interfaces.Proxies;
using Application.Interfaces.Services.Trading.Monitors;

using Binance.Net.Enums;
using Binance.Net.Interfaces.Clients.UsdFuturesApi;
using Binance.Net.Objects.Models.Futures.Socket;

using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Sockets;

using Infrastructure.Extensions;

namespace Infrastructure.Services.Trading.Monitors;

public class OrderStatusMonitor : IOrderStatusMonitor
{
    private readonly IBinanceClientUsdFuturesApiAccount Account;
    private readonly IBinanceSocketClientUsdFuturesStreams FuturesStreams;
    private readonly IUpdateSubscriptionProxy UserDataUpdatesSubscription;
    private readonly ILoggerAdapter<OrderStatusMonitor> Logger;

    public OrderStatusMonitor(IBinanceClientUsdFuturesApiAccount account, IBinanceSocketClientUsdFuturesStreams futuresStreams, IUpdateSubscriptionProxy userDataUpdatesSubscription, ILoggerAdapter<OrderStatusMonitor> logger)
    {
        this.Account = account;
        this.FuturesStreams = futuresStreams;
        this.UserDataUpdatesSubscription = userDataUpdatesSubscription;
        this.Logger = logger;
    }
    internal OrderStatusMonitor(IBinanceClientUsdFuturesApiAccount account, IBinanceSocketClientUsdFuturesStreams futuresStreams, IUpdateSubscriptionProxy userDataUpdatesSubscription, ILoggerAdapter<OrderStatusMonitor> logger, IDictionary<long, OrderStatus?> orderStatuses) : this(account, futuresStreams, userDataUpdatesSubscription, logger)
    {
        this.Orders = orderStatuses;
    }


    private readonly IDictionary<long, OrderStatus?> Orders = new Dictionary<long, OrderStatus?>();
    public bool Subscribed { get; private set; }

    public async Task SubscribeToOrderUpdatesAsync()
    {
        if (this.Subscribed)
            return;


        var listenKeyCallResult = await this.Account.StartUserStreamAsync();
        var listenKey = listenKeyCallResult.Data;

        var callResult = await this.FuturesStreams.SubscribeToUserDataUpdatesAsync(listenKey, null!, null!, null!, this.HandleOrderUpdate, null!, null!, null!);
        callResult.ThrowIfHasError("Could not subscribe to user data updates");

        this.UserDataUpdatesSubscription.SetSubscription(callResult.Data);
        this.UserDataUpdatesSubscription.ConnectionLost += async () => await this.UserDataUpdatesSubscription_ConnectionLost();
        this.Subscribed = true;
    }
    private async Task UserDataUpdatesSubscription_ConnectionLost()
    {
        this.Subscribed = false;
        this.Logger.LogInformation("Connection to {0} has been lost, attempting to reconnect", nameof(this.UserDataUpdatesSubscription));
        await this.UserDataUpdatesSubscription.ReconnectAsync();
        this.Subscribed = true;
    }
    internal void HandleOrderUpdate(DataEvent<BinanceFuturesStreamOrderUpdate> dataEvent)
    {
        var order = dataEvent.Data.UpdateData;

        if (this.Orders.ContainsKey(order.OrderId))
            this.Orders[order.OrderId] = order.Status;

        // // TODO optimization possibly with CollectionsMarshal // //
    }

    public async Task UnsubscribeFromOrderUpdatesAsync()
    {
        await this.UserDataUpdatesSubscription.CloseAsync();
        this.Subscribed = false;
    }

    public async ValueTask<OrderStatus> GetStatusAsync(long OrderID)
    {
        if (!this.Orders.ContainsKey(OrderID))
            throw new KeyNotFoundException($"The given key '{OrderID}' was not present in the dictionary.");

        while (this.Orders[OrderID] is null)
            await Task.Delay(20);

        return await new ValueTask<OrderStatus>(this.Orders[OrderID]!.Value);

        // // TODO optimization // //
    }

    public async Task WaitForOrderToReachStatusAsync(long OrderID, OrderStatus OrderStatus)
    {
        if (!this.Subscribed)
            throw new Exception("Not subscribed to user data updates");

        if (!this.Orders.ContainsKey(OrderID))
            this.Orders[OrderID] = null;

        while (this.Orders[OrderID] != OrderStatus)
            await Task.Delay(50);

        // // TODO optimization (ex: CollectionsMarshal.GetValueRefOrNullRef(...)) // //
    }

    public async Task WaitForAnyOrderToReachStatusAsync(IEnumerable<long> OrderIDs, OrderStatus OrderStatus)
    {
        if (!this.Subscribed)
            throw new Exception("Not subscribed to user data updates");
        
        foreach (var orderId in OrderIDs)
            if (!this.Orders.ContainsKey(orderId))
                this.Orders[orderId] = null;
        
        while (!this.Orders.Values.Any(x => x == OrderStatus))
            await Task.Delay(50);

        // // TODO optimization (ex: CollectionsMarshal.GetValueRefOrNullRef(...)) // //
    }
}
