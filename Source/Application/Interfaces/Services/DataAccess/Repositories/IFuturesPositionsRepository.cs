using Domain.Models.Orders;

namespace Application.Interfaces.Services.DataAccess.Repositories;

public interface IFuturesPositionsRepository
{
    public Task AddFuturesPositionAsync(FuturesPosition position, IEnumerable<FuturesOrder> futuresOrders);
    public Task<IEnumerable<FuturesPosition>> GetAllFuturesPositionsAsync();
    public Task<IEnumerable<FuturesPosition>> GetFuturesPositionsByCurrencyPairAsync(string currencyPair);
    public Task UpdateFuturesPositionAsync(Guid positionId, FuturesPosition updatedPosition);
}
