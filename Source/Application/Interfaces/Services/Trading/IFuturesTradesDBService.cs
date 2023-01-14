using Binance.Net.Objects.Models.Futures;

using Skender.Stock.Indicators;

namespace Application.Interfaces.Services.Trading;

public interface IFuturesTradesDBService<T> where T : IQuote
{
    public Task<bool> AddCandlestickAsync(T Candlestick);

    public Task<bool> AddFuturesOrderAsync(BinanceFuturesOrder FuturesOrder, T Candlestick);
}
