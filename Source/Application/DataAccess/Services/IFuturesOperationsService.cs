using Domain.Models.Futures;

namespace Application.DataAccess.Services;

public interface IFuturesOperationsService
{
    public Task AddFuturesPositionAndOrdersAsync(FuturesPosition position, IEnumerable<FuturesOrder> orders);
    public Task UpdateFuturesPositionAndAddOrdersAsync(FuturesPosition updatedPosition, IEnumerable<FuturesOrder> newOrders);
    public Task UpdateFuturesPositionsAndAddTheirOrdersAsync(Dictionary<FuturesPosition, IEnumerable<FuturesOrder>> positionsOrders);
}
