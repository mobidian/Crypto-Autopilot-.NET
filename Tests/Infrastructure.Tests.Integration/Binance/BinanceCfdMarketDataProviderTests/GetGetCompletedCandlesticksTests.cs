using Binance.Net.Enums;

using Infrastructure.Tests.Integration.Binance.BinanceCfdMarketDataProviderTests.Base;

namespace Infrastructure.Tests.Integration.Binance.BinanceCfdMarketDataProviderTests;

[Parallelizable(ParallelScope.All)]
public class GetGetCompletedCandlesticksTests : BinanceCfdMarketDataProviderTestsBase
{
    [Test, TestCaseSource(nameof(GetValidKlineIntervals))]
    public async Task GetAllCandlesticksAsync_ShouldReturnAllCandlesticks_WhenTimeframeIsValid(KlineInterval timeframe)
    {
        // Act
        var callTimeUtc = DateTime.UtcNow;
        var candlesticks = await SUT.GetCompletedCandlesticksAsync(this.CurrencyPair.Name, timeframe);

        // Assert
        callTimeUtc.Subtract(TimeSpan.FromSeconds(2 * (int)timeframe)).Should().BeBefore(candlesticks.Last().Date);
        CandlesticksAreTimelyConsistent(candlesticks, timeframe).Should().BeTrue();
    }


    [Test, TestCaseSource(nameof(GetInvalidKlineIntervals))]
    public async Task GetAllCandlesticksAsync_ShouldThrow_WhenTimeframeIsUnsupported(KlineInterval timeframe)
    {
        // Act
        var func = async () => await SUT.GetCompletedCandlesticksAsync(this.CurrencyPair.Name, timeframe);

        // Assert
        await func.Should().ThrowExactlyAsync<NotSupportedException>().WithMessage($"The {timeframe} timeframe is not supported");
    }
}