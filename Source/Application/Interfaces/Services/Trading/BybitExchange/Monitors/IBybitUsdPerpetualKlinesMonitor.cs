using Bybit.Net.Enums;
using Bybit.Net.Objects.Models.Socket;

namespace Application.Interfaces.Services.Trading.BybitExchange.Monitors;

public interface IBybitUsdPerpetualKlinesMonitor
{
    public Task SubscribeToKlineUpdatesAsync(string currencyPair, KlineInterval timeframe);

    public bool IsSubscribedTo(string currencyPair, KlineInterval timeframe);
    
    public Task UnsubscribeFromKlineUpdatesAsync(string currencyPair, KlineInterval timeframe);
    
    public Task<BybitKlineUpdate> WaitForNextCandlestickAsync(string currencyPair, KlineInterval timeframe);
}
