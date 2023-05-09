using Domain.Models;

namespace Application.Interfaces.Services.DataAccess;

public interface IFuturesTradesDBService
{
    public Task AddFuturesOrderAsync(FuturesOrder futuresOrder, Guid? positionId = null);
    public Task AddFuturesOrdersAsync(IEnumerable<FuturesOrder> futuresOrder, Guid? positionId = null);
    public Task<IEnumerable<FuturesOrder>> GetAllFuturesOrdersAsync();
    public Task<IEnumerable<FuturesOrder>> GetFuturesOrdersByCurrencyPairAsync(string currencyPair);
    public Task UpdateFuturesOrderAsync(Guid bybitID, FuturesOrder updatedFuturesOrder, Guid? positionId = null);
    public Task DeleteFuturesOrdersAsync(params Guid[] bybitIDs);

    public Task AddFuturesPositionAsync(FuturesPosition position, IEnumerable<FuturesOrder> futuresOrders);
    public Task<IEnumerable<FuturesPosition>> GetAllFuturesPositionsAsync();
    public Task<IEnumerable<FuturesPosition>> GetFuturesPositionsByCurrencyPairAsync(string currencyPair);
    public Task UpdateFuturesPositionAsync(Guid positionId, FuturesPosition updatedPosition);
}
