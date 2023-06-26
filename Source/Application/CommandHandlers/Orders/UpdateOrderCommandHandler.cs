using Application.DataAccess.Repositories;

using Domain.Commands.Orders;

using MediatR;

namespace Application.CommandHandlers.Orders;

public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, Unit>
{
    private readonly IFuturesOrdersRepository OrdersRepository;
    public UpdateOrderCommandHandler(IFuturesOrdersRepository ordersRepository) => this.OrdersRepository = ordersRepository;

    public async Task<Unit> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var bybitId = request.UpdatedOrder.BybitID;
        var updatedLimitOrder = request.UpdatedOrder;

        await this.OrdersRepository.UpdateAsync(bybitId, updatedLimitOrder, request.FuturesPositionId);
        return Unit.Value;
    }
}
