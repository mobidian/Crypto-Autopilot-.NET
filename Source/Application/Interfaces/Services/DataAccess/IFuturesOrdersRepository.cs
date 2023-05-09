using Domain.Models;

namespace Application.Interfaces.Services.DataAccess;

public interface IFuturesOrdersRepository
{
    public Task AddFuturesOrderAsync(FuturesOrder futuresOrder, Guid? positionId = null);
    public Task AddFuturesOrdersAsync(IEnumerable<FuturesOrder> futuresOrder, Guid? positionId = null);
    public Task<IEnumerable<FuturesOrder>> GetAllFuturesOrdersAsync();
    public Task<IEnumerable<FuturesOrder>> GetFuturesOrdersByCurrencyPairAsync(string currencyPair);
    public Task UpdateFuturesOrderAsync(Guid bybitID, FuturesOrder updatedFuturesOrder, Guid? positionId = null);
    public Task DeleteFuturesOrdersAsync(params Guid[] bybitIDs);
}
