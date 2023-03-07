using Application.Exceptions;
using Application.Interfaces.Logging;
using Application.Interfaces.Proxies;
using Application.Interfaces.Services.Trading.Bybit.Monitors;

using Bybit.Net.Clients.UsdPerpetualApi;
using Bybit.Net.Enums;
using Bybit.Net.Interfaces.Clients.UsdPerpetualApi;
using Bybit.Net.Objects.Models.Socket;

using CryptoExchange.Net.Sockets;

using Infrastructure.Extensions;

namespace Infrastructure.Services.Trading.Bybit.Monitors;

public class ByBitUsdPerpetualOrderMonitor : IByBitUsdPerpetualOrderMonitor
{
    private readonly IBybitSocketClientUsdPerpetualStreams UsdPerpetualStreams;
    private readonly IUpdateSubscriptionProxy UsdPerpetualUpdatesSubscription;
    private readonly ILoggerAdapter<ByBitUsdPerpetualOrderMonitor> Logger;

    public ByBitUsdPerpetualOrderMonitor(IBybitSocketClientUsdPerpetualStreams usdPerpetualStreams, IUpdateSubscriptionProxy usdPerpetualUpdatesSubscription, ILoggerAdapter<ByBitUsdPerpetualOrderMonitor> logger) : this(usdPerpetualStreams, usdPerpetualUpdatesSubscription, logger, new Dictionary<Guid, OrderStatus?>()) { }
    public ByBitUsdPerpetualOrderMonitor(IBybitSocketClientUsdPerpetualStreams usdPerpetualStreams, IUpdateSubscriptionProxy usdPerpetualUpdatesSubscription, ILoggerAdapter<ByBitUsdPerpetualOrderMonitor> logger, IDictionary<Guid, OrderStatus?> orders)
    {
        this.UsdPerpetualStreams = usdPerpetualStreams ?? throw new ArgumentNullException(nameof(usdPerpetualStreams));
        this.UsdPerpetualUpdatesSubscription = usdPerpetualUpdatesSubscription ?? throw new ArgumentNullException(nameof(usdPerpetualUpdatesSubscription));
        this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.Orders = orders ?? throw new ArgumentNullException(nameof(orders));
    }
    

    
    private readonly IDictionary<Guid, OrderStatus?> Orders;
    public bool Subscribed { get; private set; }

    public async Task SubscribeToOrderUpdatesAsync()
    {
        if (this.Subscribed)
            return;


        var callResult = await this.UsdPerpetualStreams.SubscribeToOrderUpdatesAsync(this.HandleUsdPerpetualOrderUpdate);
        callResult.ThrowIfHasError("Could not subscribe to user USD Perpetual order updates");
        
        this.UsdPerpetualUpdatesSubscription.SetSubscription(callResult.Data);
        this.Subscribed = true;
    }
    internal void HandleUsdPerpetualOrderUpdate(DataEvent<IEnumerable<BybitUsdPerpetualOrderUpdate>> dataEvent)
    {
        var perpetualOrder = dataEvent.Data.Last();
        this.Orders[Guid.Parse(perpetualOrder.Id)] = perpetualOrder.Status;
    }
    
    public async Task UnsubscribeFromOrderUpdatesAsync()
    {
        if (!this.Subscribed)
            throw new NotSubscribedException("Not subscribed to perpetual order updates");

        await this.UsdPerpetualStreams.UnsubscribeAsync(this.UsdPerpetualUpdatesSubscription.Id);
        this.Subscribed = false;
    }

    public async Task WaitForOrderToReachStatusAsync(Guid orderID, OrderStatus orderStatus, CancellationToken token = default)
    {
        if (!this.Subscribed)
            throw new NotSubscribedException("Not subscribed to perpetual order updates");

        this.Orders.TryAdd(orderID, null);
        
        while (this.Orders[orderID] != orderStatus && this.Subscribed)
            await Task.Delay(50, token);
        
        this.ThrowIfConsumerUnsubscribed(orderStatus);
    }
    public async Task<Guid> WaitForAnyOrderToReachStatusAsync(IEnumerable<Guid> orderIDs, OrderStatus orderStatus, CancellationToken token = default)
    {
        if (!this.Subscribed)
            throw new NotSubscribedException("Not subscribed to perpetual order updates");

        foreach (var orderId in orderIDs)
            this.Orders.TryAdd(orderId, null);

        while (!this.Orders.Values.Any(x => x == orderStatus) && this.Subscribed)
            await Task.Delay(50, token);
        
        this.ThrowIfConsumerUnsubscribed(orderStatus);

        return this.Orders.First(x => x.Value == orderStatus).Key;
    }

    private void ThrowIfConsumerUnsubscribed(OrderStatus orderStatus)
    {
        if (this.Subscribed)
            return;
        
        var exceptionMessage = $"The operation of waiting for any of the orders with the specified IDs to reach {orderStatus} has been cancelled as a consequence of the consumer unsubscribing from order updates";
        throw new TaskCanceledException(exceptionMessage, new NotSubscribedException("Not subscribed to perpetual order updates"));
    }
}
