using Binance.Net.Objects.Models.Futures.Socket;
using Binance.Net.Objects.Models;

using CryptoExchange.Net.Sockets;

using Infrastructure.Tests.Unit.OrderStatusMonitorTests.Base;

namespace Infrastructure.Tests.Unit.OrderStatusMonitorTests;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[Parallelizable(ParallelScope.All)]
public class SubscribeToOrderUpdatesTests : OrderStatusMonitorTestsBase
{
    [Test]
    public async Task SubscribeToOrderUpdatesAsync_ShouldSubscribeToOrderUpdates_WhenUnsubscribed()
    {
        // Act
        await this.SUT.SubscribeToOrderUpdatesAsync();
        
        // Assert
        await this.Account.Received(1).StartUserStreamAsync();
        await this.FuturesStreams.Received(1).SubscribeToUserDataUpdatesAsync(
                listenKey: Arg.Is<string>(this.listenKey),
                onLeverageUpdate: Arg.Any<Action<DataEvent<BinanceFuturesStreamConfigUpdate>>>(),
                onMarginUpdate: Arg.Any<Action<DataEvent<BinanceFuturesStreamMarginUpdate>>>(),
                onAccountUpdate: Arg.Any<Action<DataEvent<BinanceFuturesStreamAccountUpdate>>>(),
                onOrderUpdate: Arg.Any<Action<DataEvent<BinanceFuturesStreamOrderUpdate>>>(),
                onListenKeyExpired: Arg.Any<Action<DataEvent<BinanceStreamEvent>>>(),
                onStrategyUpdate: Arg.Any<Action<DataEvent<BinanceStrategyUpdate>>>(),
                onGridUpdate: Arg.Any<Action<DataEvent<BinanceGridUpdate>>>());
        this.UserDataUpdatesSubscription.Received(1).SetSubscription(Arg.Any<UpdateSubscription>());
        this.SUT.Subscribed.Should().BeTrue();
    }
}
