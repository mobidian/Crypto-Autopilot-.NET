using Binance.Net.Enums;
using Binance.Net.Interfaces;

using Domain.Models;

namespace Infrastructure.Services.Trading;

public interface IFuturesMarketsCandlestickAwaiter
{
    public CurrencyPair CurrencyPair { get; }
    public KlineInterval Timeframe { get; }

    public bool SubscribedToKlineUpdates { get; }


    public Task SubscribeToKlineUpdatesAsync();
    public Task UnsubscribeFromKlineUpdatesAsync();
    public Task<IBinanceStreamKlineData> WaitForNextCandlestickAsync();
}
