using Binance.Net.Enums;

using Domain.Models;

using FluentAssertions;

using Infrastructure.Tests.Integration.BinanceCfdMarketDataProviderTests.Common;

namespace Infrastructure.Tests.Integration.BinanceCfdMarketDataProviderTests;

public class GetAllCandlesticksTests : BinanceCfdMarketDataProviderTestsBase
{
    [Test, TestCaseSource(nameof(GetValidKlineIntervals))]
    public async Task GetAllCandlesticksAsync_ShouldReturnAllCandlesticks_WhenTimeframeIsValid(KlineInterval timeframe)
    {
        // Act
        var candlesticks = await SUT.GetAllCandlesticksAsync(timeframe);

        // Assert
        candlesticks.Last().Date.Add(TimeSpan.FromSeconds((int)timeframe)).Should().BeAfter(DateTime.UtcNow);
        base.AreCandlesticksTimelyConsistent(candlesticks, timeframe).Should().BeTrue();
    }


    [Test, TestCaseSource(nameof(GetInvalidKlineIntervals))]
    public async Task GetAllCandlesticksAsync_ShouldThrow_WhenTimeframeIsUnsupported(KlineInterval timeframe)
    {
        // Act
        var func = async () => await SUT.GetAllCandlesticksAsync(timeframe);

        // Assert
        await func.Should().ThrowExactlyAsync<NotSupportedException>().WithMessage($"The {timeframe} timeframe is not supported");
    }
}
