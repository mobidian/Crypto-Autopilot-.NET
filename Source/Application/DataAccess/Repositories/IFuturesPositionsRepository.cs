using Domain.Models.Futures;

namespace Application.DataAccess.Repositories;

public interface IFuturesPositionsRepository
{
    public Task AddAsync(FuturesPosition position);
    public Task AddAsync(IEnumerable<FuturesPosition> positions);


    public Task<FuturesPosition?> GetByCryptoAutopilotId(Guid cryptoAutopilotId);
    public Task<IEnumerable<FuturesPosition>> GetAllAsync();
    public Task<IEnumerable<FuturesPosition>> GetByCurrencyPairAsync(string currencyPair);

    public Task UpdateAsync(FuturesPosition updatedPosition);
}
