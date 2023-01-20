using Binance.Net.Interfaces;

using CryptoExchange.Net.Sockets;

using Infrastructure.Tests.Unit.FuturesMarketsObserverTests.Base;

namespace Infrastructure.Tests.Unit.FuturesMarketsObserverTests;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class KlineUpdatesSubscriptionTests : FuturesMarketsCandlestickAwaiterTestsBase
{
    [Test]
    public async Task WaitForNewCandlestick_ShouldWaitAndReturnCandlestick_WhenSubscribed()
    {
        // Arrange
        DateTime dateTime = DateTime.Now;
        var newCandlestickDataEvent = CreateDataEvent(dateTime.AddHours(1));
        var dataEvents = Enumerable.Range(0, 100).Select(_ => CreateDataEvent(dateTime)).ToList();

        await this.SUT_SubscribeToKlineUpdatesAsync(dataEvents.First());
        await Task.Delay(this.RNG.Next(50, 100));
        

        // Act
        var task = this.SUT.WaitForNextCandlestickAsync();
        await Task.Delay(this.RNG.Next(50, 100));
        bool completedBeforeReceivingDataEvents = task.IsCompleted;
        
        dataEvents.ForEach(this.SUT.HandleKlineUpdate);
        await Task.Delay(this.RNG.Next(50, 100));
        bool completedBeforeReceivingNewCandlestick = task.IsCompleted;

        this.SUT.HandleKlineUpdate(newCandlestickDataEvent);
        await Task.Delay(this.RNG.Next(50, 100));
        bool completedAfterReceivingNewCandlestick = task.IsCompleted;

        
        // Assert
        completedBeforeReceivingDataEvents.Should().BeFalse();
        completedBeforeReceivingNewCandlestick.Should().BeFalse();
        completedAfterReceivingNewCandlestick.Should().BeTrue();
        task.Result.Should().Be(newCandlestickDataEvent.Data);
    }
    
    [Test]
    public async Task WaitForNewCandlestick_ShouldWaitAndReturnCandlestickForMultipleSequentialCandlesticksAtEverInvoke()
    {
        // Arrange
        var listOfDataEventLists = new List<List<DataEvent<IBinanceStreamKlineData>>>();
        var ascendingDatetimes = Enumerable.Range(0, 5).Select(i => DateTime.Now.AddHours(i)).ToList();
        for (int i = 0; i < ascendingDatetimes.Count - 1; i++)
            listOfDataEventLists.Add(Enumerable.Range(0, 5).Select(_ => CreateDataEvent(ascendingDatetimes[i])).Append(CreateDataEvent(ascendingDatetimes[i + 1])).ToList());

        // Act
        await this.SUT_SubscribeToKlineUpdatesAsync(listOfDataEventLists.First().First());
        foreach (var dataEventList in listOfDataEventLists)
        {
            // Act
            var task = this.SUT.WaitForNextCandlestickAsync();
            
            dataEventList.ForEach(this.SUT.HandleKlineUpdate);
            Thread.Sleep(1000);

            // Assert
            task.IsCompleted.Should().BeTrue();
            task.Result.Should().Be(dataEventList.Last().Data);
        }
    }

    

    [Test]
    public async Task WaitForNewCandlestick_ShouldThrow_WhenNotSubscribed()
    {
        // Act
        var func = this.SUT.WaitForNextCandlestickAsync;
        
        // Assert
        await func.Should().ThrowExactlyAsync<Exception>().WithMessage("Not subscribed to kline updates");
    }



    [Test]
    public async Task UnsubscribeFromKlineUpdatesAsync_ShouldUnsubscribe_WhenSubscribed()
    {
        // Arrange
        await this.SUT_SubscribeToKlineUpdatesAsync(DateTime.Now);
        bool subscribedAfterSubscribe = this.SUT.SubscribedToKlineUpdates;

        // Act
        await this.SUT.UnsubscribeFromKlineUpdatesAsync();
        bool subscribedAfterUnsubscribe = this.SUT.SubscribedToKlineUpdates;
        var func = this.SUT.WaitForNextCandlestickAsync;

        // Assert
        subscribedAfterSubscribe.Should().BeTrue();
        await func.Should().ThrowExactlyAsync<Exception>().WithMessage("Not subscribed to kline updates");
        subscribedAfterUnsubscribe.Should().BeFalse();
    }
}
