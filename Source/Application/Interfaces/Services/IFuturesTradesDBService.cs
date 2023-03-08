using Domain.Models;

namespace Application.Interfaces.Services;

public interface IFuturesTradesDBService
{
    public Task AddFuturesOrdersAsync(params FuturesOrder[] FuturesOrders);

    public Task<IEnumerable<FuturesOrder>> GetAllFuturesOrdersAsync();
    public Task<IEnumerable<FuturesOrder>> GetFuturesOrdersByCurrencyPairAsync(string currencyPair);

    public Task UpdateFuturesOrderAsync(Guid uniqueID, FuturesOrder futuresOrder);
}
