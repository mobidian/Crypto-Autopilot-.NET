using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Models.Spot.Socket;

using Bogus;

using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;

using FluentAssertions;

using Infrastructure.Tests.Unit.FuturesMarketsObserverTests.Base;

using NSubstitute;

namespace Infrastructure.Tests.Unit.FuturesMarketsObserverTests;

public class KlineUpdatesSubscriptionTests : FuturesMarketsObserverTestsBase
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        this.FuturesStreams
            .SubscribeToKlineUpdatesAsync(
                Arg.Any<string>(),
                Arg.Any<KlineInterval>(),
                Arg.Any<Action<DataEvent<IBinanceStreamKlineData>>>())
            .Returns(Task.FromResult(new CallResult<UpdateSubscription>(new UpdateSubscription(null!, null!))));
    }
    
    
    [Test]
    public async Task WaitForNewCandlestick_ShouldWaitAndReturnCandlestick_WhenSubscribed()
    {
        // Act
        await this.SUT.SubscribeToKlineUpdatesAsync();
        Task<IBinanceStreamKlineData> task = this.SUT.WaitForNewCandlestickAsync();

        // Act
        Enumerable.Range(0, 20).Select(_ => CreateDataEvent(DateTime.MinValue)).ToList().ForEach(this.SUT.HandleKlineUpdate);
        bool completedBeforeNewCandle = task.IsCompleted;
        
        var kline = new BinanceStreamKline();
        this.SUT.HandleKlineUpdate(CreateDataEvent(DateTime.MaxValue, kline));
        await Task.Delay(50);
        bool completedAfterNewCandle = task.IsCompleted;
        
        // Assert
        completedBeforeNewCandle.Should().BeFalse();
        completedAfterNewCandle.Should().BeTrue();
        task.IsFaulted.Should().BeFalse();
        task.Result.Data.Should().Be(kline);
    } 
    private static DataEvent<IBinanceStreamKlineData> CreateDataEvent(DateTime KlineOpenTime, BinanceStreamKline kline = default!)
    {
        var dataEvent = new DataEvent<IBinanceStreamKlineData>(new BinanceStreamKlineData(), DateTime.MinValue);
        dataEvent.Data.Data = kline ?? new BinanceStreamKline();
        dataEvent.Data.Data.OpenTime = KlineOpenTime;
        return dataEvent;
    }
    
    [Test]
    public async Task WaitForNewCandlestick_ShouldThrow_WhenNotSubscribed()
    {
        // Act
        var func = this.SUT.WaitForNewCandlestickAsync;
        
        // Assert
        await func.Should().ThrowExactlyAsync<Exception>().WithMessage("Not subscribed to kline updates");
    }
    
    [Test]
    [Ignore("At the moment the SUT depends on an UpdateSubscription object, not an interface so the UnsubscribeFromKlineUpdatesAsync method is untestable")]
    public async Task UnsubscribeFromKlineUpdatesAsync_ShouldUnsubscribe_WhenSubscribed()
    {
        // Arrange
        await this.SUT.SubscribeToKlineUpdatesAsync();
        bool subscribedAfterSubscribe = this.SUT.SubscribedToKlineUpdates;

        // Act
        await this.SUT.UnsubscribeFromKlineUpdatesAsync();
        bool subscribedAfterUnsubscribe = this.SUT.SubscribedToKlineUpdates;
        var func = this.SUT.WaitForNewCandlestickAsync;

        // Assert
        subscribedAfterSubscribe.Should().BeFalse();
        await func.Should().ThrowExactlyAsync<Exception>().WithMessage("Not subscribed to kline updates");
        subscribedAfterUnsubscribe.Should().BeTrue();
    }
}
