using Binance.Net.Objects.Models.Futures;

using Domain.Models;

namespace Application.Interfaces.Services.Trading;

public interface IFuturesTradesDBService
{
    public Task AddCandlestickAsync(Candlestick Candlestick);
    
    public Task AddFuturesOrderAsync(BinanceFuturesOrder FuturesOrder, Candlestick Candlestick);

    public Task<IEnumerable<Candlestick>> GetAllCandlesticksAsync();
    public Task<IEnumerable<Candlestick>> GetCandlesticksByCurrencyPairAsync(string currencyPair);

    public Task<IEnumerable<BinanceFuturesOrder>> GetAllFuturesOrdersAsync();
    public Task<IEnumerable<BinanceFuturesOrder>> GetFuturesOrdersByCurrencyPairAsync(string currencyPair);
}
