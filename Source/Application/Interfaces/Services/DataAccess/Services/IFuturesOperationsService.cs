using Domain.Models.Futures;

namespace Application.Interfaces.Services.DataAccess.Services;

public interface IFuturesOperationsService
{
    public Task AddFuturesPositionAndOrdersAsync(FuturesPosition position, IEnumerable<FuturesOrder> orders);
    public Task UpdateFuturesPositionAndAddOrdersAsync(Guid cryptoAutopilotId, FuturesPosition updatedPosition, IEnumerable<FuturesOrder> newOrders);
}
