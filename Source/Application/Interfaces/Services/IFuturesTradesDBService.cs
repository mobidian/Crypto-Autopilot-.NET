using Domain.Models;

namespace Application.Interfaces.Services;

public interface IFuturesTradesDBService
{
    public Task AddCandlestickAsync(Candlestick Candlestick);

    public Task AddFuturesOrdersAsync(Candlestick Candlestick, params FuturesOrder[] FuturesOrders);

    public Task<IEnumerable<Candlestick>> GetAllCandlesticksAsync();
    public Task<IEnumerable<Candlestick>> GetCandlesticksByCurrencyPairAsync(string currencyPair);

    public Task<IEnumerable<FuturesOrder>> GetAllFuturesOrdersAsync();
    public Task<IEnumerable<FuturesOrder>> GetFuturesOrdersByCurrencyPairAsync(string currencyPair);

    public Task UpdateFuturesOrderAsync(Guid uniqueID, FuturesOrder futuresOrder);
}
