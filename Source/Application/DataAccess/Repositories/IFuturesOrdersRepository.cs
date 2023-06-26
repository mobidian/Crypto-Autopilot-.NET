using Domain.Models.Futures;

namespace Application.DataAccess.Repositories;

public interface IFuturesOrdersRepository
{
    public Task AddAsync(FuturesOrder futuresOrder, Guid? positionId = null);
    public Task AddAsync(IEnumerable<FuturesOrder> futuresOrder, Guid? positionId = null);


    public Task<FuturesOrder?> GetByBybitId(Guid bybitID);
    public Task<IEnumerable<FuturesOrder>> GetAllAsync();
    public Task<IEnumerable<FuturesOrder>> GetByCurrencyPairAsync(string currencyPair);

    public Task UpdateAsync(FuturesOrder updatedFuturesOrder, Guid? positionId = null);

    public Task DeleteAsync(params Guid[] bybitIDs);
}
