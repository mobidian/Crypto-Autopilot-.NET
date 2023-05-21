using Domain.Models.Futures;

namespace Application.Interfaces.Services.DataAccess.Repositories;

public interface IFuturesPositionsRepository
{
    public Task AddFuturesPositionAsync(FuturesPosition position);
    public Task AddFuturesPositionsAsync(IEnumerable<FuturesPosition> positions);

    
    public Task<FuturesPosition?> GetFuturesOrderByCryptoAutopilotId(Guid cryptoAutopilotId);
    public Task<IEnumerable<FuturesPosition>> GetAllFuturesPositionsAsync();
    public Task<IEnumerable<FuturesPosition>> GetFuturesPositionsByCurrencyPairAsync(string currencyPair);

    public Task UpdateFuturesPositionAsync(Guid positionId, FuturesPosition updatedPosition);
}
