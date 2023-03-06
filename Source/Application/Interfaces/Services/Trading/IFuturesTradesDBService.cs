using Binance.Net.Objects.Models.Futures;

using Domain.Models;

namespace Application.Interfaces.Services.Trading.Binance;

public interface IFuturesTradesDBService
{
    public Task AddCandlestickAsync(Candlestick Candlestick);

    public Task AddFuturesOrdersAsync(Candlestick Candlestick, params BinanceFuturesOrder[] FuturesOrders);

    public Task<IEnumerable<Candlestick>> GetAllCandlesticksAsync();
    public Task<IEnumerable<Candlestick>> GetCandlesticksByCurrencyPairAsync(string currencyPair);

    public Task<IEnumerable<BinanceFuturesOrder>> GetAllFuturesOrdersAsync();
    public Task<IEnumerable<BinanceFuturesOrder>> GetFuturesOrdersByCurrencyPairAsync(string currencyPair);
}
