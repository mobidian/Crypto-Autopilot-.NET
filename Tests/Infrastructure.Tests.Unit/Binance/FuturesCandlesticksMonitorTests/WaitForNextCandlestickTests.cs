using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures.Socket;
using Binance.Net.Objects.Models.Spot.Socket;

using CryptoExchange.Net.Sockets;

using FluentAssertions.Extensions;

using Infrastructure.Tests.Unit.Binance.FuturesCandlesticksMonitorTests.Base;

namespace Infrastructure.Tests.Unit.Binance.FuturesCandlesticksMonitorTests;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[Parallelizable(ParallelScope.All)]
public class WaitForNextCandlestickTests : FuturesCandlesticksMonitorTestsBase
{
    [Test]
    public async Task WaitForNextCandlestickAsync_ShouldWaitForNewCandlestick_WhenSubscribed()
    {
        // Arrange
        var contract = this.GetRandomContractIdentifier();
        await this.SUT.SubscribeToKlineUpdatesAsync(contract.currencyPair, contract.contractType, contract.timeframe);

        var initialOpenTime = DateTime.Now.Date.AddHours(DateTime.Now.Hour).AsUtc();
        var newOpenTime = initialOpenTime.AddSeconds((int)contract.timeframe).AsUtc();

        var randomTime = GetRandomTimeBetween(initialOpenTime, newOpenTime); // this is considered to be the time at which the WaitForNextCandlestickAsync method is invoked
        this.DateTimeProvider.UtcNow.Returns(randomTime);

        var dataEvents = Enumerable.Range(0, 10).Select(i => CreateDataEvent(contract, initialOpenTime)).ToList();
        var newCandlestickDataEvent = CreateDataEvent(contract, newOpenTime);


        // Act
        var task = this.SUT.WaitForNextCandlestickAsync(contract.currencyPair, contract.contractType, contract.timeframe);
        await Task.Delay(50);
        var taskCompletedBeforeFirstUpdate = task.IsCompleted;

        dataEvents.ForEach(this.SUT.HandleContractKlineUpdate);
        await Task.Delay(50);
        var taskCompletedBeforeNewCandlestickUpdate = task.IsCompleted;

        this.SUT.HandleContractKlineUpdate(newCandlestickDataEvent);
        await Task.Delay(50);
        var taskCompletedAfterNewCandlestickUpdate = task.IsCompleted;


        // Assert
        taskCompletedBeforeFirstUpdate.Should().BeFalse();
        taskCompletedBeforeNewCandlestickUpdate.Should().BeFalse();
        taskCompletedAfterNewCandlestickUpdate.Should().BeTrue();
    }

    [Test]
    public async Task WaitForNextCandlestickAsync_ShouldWaitForNewCandlestick_WhenSubscribedAndTheFirstUpdateIsTheNewCandleUpdate()
    {
        // Arrange
        var contract = this.GetRandomContractIdentifier();
        await this.SUT.SubscribeToKlineUpdatesAsync(contract.currencyPair, contract.contractType, contract.timeframe);

        var newOpenTime = DateTime.Now.Date.AddHours(DateTime.Now.Hour).AsUtc();
        var randomTime = GetRandomTimeBetween(newOpenTime.AddSeconds(-1), newOpenTime); // this is considered to be the time at which the WaitForNextCandlestickAsync method is invoked
        this.DateTimeProvider.UtcNow.Returns(randomTime);

        var newCandlestickDataEvent = CreateDataEvent(contract, newOpenTime);


        // Act
        var task = this.SUT.WaitForNextCandlestickAsync(contract.currencyPair, contract.contractType, contract.timeframe);
        await Task.Delay(50);
        var taskCompletedBeforeFirstUpdate = task.IsCompleted;

        this.SUT.HandleContractKlineUpdate(newCandlestickDataEvent);
        await Task.Delay(50);
        var taskCompletedAfterNewCandlestickUpdate = task.IsCompleted;


        // Assert
        taskCompletedBeforeFirstUpdate.Should().BeFalse();
        taskCompletedAfterNewCandlestickUpdate.Should().BeTrue();
    }


    private static DateTime GetRandomTimeBetween(DateTime minTime, DateTime maxTime) => minTime + TimeSpan.FromMicroseconds(Random.Shared.NextDouble() * (maxTime - minTime).TotalMicroseconds);
    private static DataEvent<BinanceStreamContinuousKlineData> CreateDataEvent((string currencyPair, ContractType contractType, KlineInterval timeframe) key, DateTime OpenTime)
    {
        var streamKline = new BinanceStreamKline();
        streamKline.Interval = key.timeframe;
        streamKline.OpenTime = OpenTime;

        var streamKlineData = new BinanceStreamContinuousKlineData();
        streamKlineData.Symbol = key.currencyPair;
        streamKlineData.ContractType = key.contractType;
        streamKlineData.Data = streamKline;

        return new DataEvent<BinanceStreamContinuousKlineData>(streamKlineData, DateTime.MinValue);
    }
}
