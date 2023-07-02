using Application.Interfaces.Logging;
using Application.Interfaces.Proxies;

using Bybit.Net.Enums;
using Bybit.Net.Interfaces.Clients.UsdPerpetualApi;
using Bybit.Net.Objects.Models.Socket;

using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;

using Infrastructure.Services.Bybit.Monitors;

using NSubstitute;

namespace Infrastructure.Tests.Unit.Bybit.ByBitUsdPerpetualOrderMonitorTests.AbstractBase;

public abstract class ByBitUsdPerpetualOrderMonitorTestsBase
{
    protected readonly ByBitUsdPerpetualOrderMonitor SUT;

    protected readonly IBybitSocketClientUsdPerpetualApi UsdPerpetualStreams = Substitute.For<IBybitSocketClientUsdPerpetualApi>();
    protected readonly IUpdateSubscriptionProxy UsdPerpetualUpdatesSubscription = Substitute.For<IUpdateSubscriptionProxy>();
    protected readonly ILoggerAdapter<ByBitUsdPerpetualOrderMonitor> Logger = Substitute.For<ILoggerAdapter<ByBitUsdPerpetualOrderMonitor>>();
    protected readonly IDictionary<Guid, OrderStatus?> Orders = new Dictionary<Guid, OrderStatus?>();

    protected readonly CallResult<UpdateSubscription> UpdateSubscriptionCallResult = new(new UpdateSubscription(null!, null!));
    
    public ByBitUsdPerpetualOrderMonitorTestsBase()
    {
        this.UsdPerpetualStreams.SubscribeToOrderUpdatesAsync(Arg.Any<Action<DataEvent<IEnumerable<BybitUsdPerpetualOrderUpdate>>>>()).Returns(this.UpdateSubscriptionCallResult);
        this.SUT = new ByBitUsdPerpetualOrderMonitor(this.UsdPerpetualStreams, this.UsdPerpetualUpdatesSubscription, this.Logger, this.Orders);
    }

    
    protected DataEvent<IEnumerable<BybitUsdPerpetualOrderUpdate>> CreateDataEvent(Guid orderId, OrderStatus orderStatus)
    {
        var orderUpdate = new BybitUsdPerpetualOrderUpdate
        {
            Id = orderId.ToString(),
            Status = orderStatus,
        };
        
        return new DataEvent<IEnumerable<BybitUsdPerpetualOrderUpdate>>(new[] { orderUpdate }, DateTime.MinValue);
    }
}
