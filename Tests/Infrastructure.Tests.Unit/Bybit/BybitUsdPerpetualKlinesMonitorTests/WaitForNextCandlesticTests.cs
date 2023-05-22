using Bybit.Net.Enums;
using Bybit.Net.Objects.Models.Socket;

using CryptoExchange.Net.Sockets;

using FluentAssertions;
using FluentAssertions.Extensions;

using Infrastructure.Tests.Unit.Bybit.BybitUsdPerpetualKlinesMonitorTests.AbstractBase;

using NSubstitute;

using Xunit;

namespace Infrastructure.Tests.Unit.Bybit.BybitUsdPerpetualKlinesMonitorTests;

public class WaitForNextCandlesticTests : BybitUsdPerpetualKlinesMonitorTestsBase
{
    [Fact]
    public async Task WaitForNextCandlestickAsync_ShouldWaitForNewCandlestick_WhenSubscribed()
    {
        // Arrange
        var contract = this.GetRandomContractIdentifier();
        await this.SUT.SubscribeToKlineUpdatesAsync(contract.currencyPair, contract.timeframe);

        var initialOpenTime = DateTime.Now.Date.AddHours(DateTime.Now.Hour).AsUtc();
        var newOpenTime = initialOpenTime.AddSeconds((int)contract.timeframe).AsUtc();

        var randomTime = GetRandomTimeBetween(initialOpenTime, newOpenTime); // this is considered to be the time at which the WaitForNextCandlestickAsync method is invoked
        this.DateTimeProvider.UtcNow.Returns(randomTime);

        var dataEvents = Enumerable.Range(0, 10).Select(i => CreateDataEvent(contract, initialOpenTime)).ToList();
        var newCandlestickDataEvent = CreateDataEvent(contract, newOpenTime);


        // Act
        var task = this.SUT.WaitForNextCandlestickAsync(contract.currencyPair, contract.timeframe);
        await Task.Delay(50);
        var taskCompletedBeforeFirstUpdate = task.IsCompleted;

        dataEvents.ForEach(this.SUT.HandleKlineUpdate);
        await Task.Delay(50);
        var taskCompletedBeforeNewCandlestickUpdate = task.IsCompleted;

        this.SUT.HandleKlineUpdate(newCandlestickDataEvent);
        await Task.Delay(50);
        var taskCompletedAfterNewCandlestickUpdate = task.IsCompleted;


        // Assert
        taskCompletedBeforeFirstUpdate.Should().BeFalse();
        taskCompletedBeforeNewCandlestickUpdate.Should().BeFalse();
        taskCompletedAfterNewCandlestickUpdate.Should().BeTrue();
    }

    [Fact]
    public async Task WaitForNextCandlestickAsync_ShouldWaitForNewCandlestick_WhenSubscribedAndTheFirstUpdateIsTheNewCandleUpdate()
    {
        // Arrange
        var contract = this.GetRandomContractIdentifier();
        await this.SUT.SubscribeToKlineUpdatesAsync(contract.currencyPair, contract.timeframe);

        var newOpenTime = DateTime.Now.Date.AddHours(DateTime.Now.Hour).AsUtc();
        var randomTime = GetRandomTimeBetween(newOpenTime.AddSeconds(-1), newOpenTime); // this is considered to be the time at which the WaitForNextCandlestickAsync method is invoked
        this.DateTimeProvider.UtcNow.Returns(randomTime);

        var newCandlestickDataEvent = CreateDataEvent(contract, newOpenTime);


        // Act
        var task = this.SUT.WaitForNextCandlestickAsync(contract.currencyPair, contract.timeframe);
        await Task.Delay(50);
        var taskCompletedBeforeFirstUpdate = task.IsCompleted;

        this.SUT.HandleKlineUpdate(newCandlestickDataEvent);
        await Task.Delay(50);
        var taskCompletedAfterNewCandlestickUpdate = task.IsCompleted;


        // Assert
        taskCompletedBeforeFirstUpdate.Should().BeFalse();
        taskCompletedAfterNewCandlestickUpdate.Should().BeTrue();
    }


    private static DateTime GetRandomTimeBetween(DateTime minTime, DateTime maxTime) => minTime + TimeSpan.FromMicroseconds(Random.Shared.NextDouble() * (maxTime - minTime).TotalMicroseconds);
    private static DataEvent<IEnumerable<BybitKlineUpdate>> CreateDataEvent((string currencyPair, KlineInterval timeframe) key, DateTime OpenTime)
    {
        var topic = $"{(int)key.timeframe / 60}.{key.currencyPair}";
        var bybitkline = new BybitKlineUpdate { OpenTime = OpenTime };

        return new DataEvent<IEnumerable<BybitKlineUpdate>>(new[] { bybitkline }, DateTime.MinValue) { Topic = topic };
    }
}
