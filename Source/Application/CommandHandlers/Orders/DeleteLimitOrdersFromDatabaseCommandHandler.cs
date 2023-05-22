using Application.Interfaces.Services.DataAccess.Repositories;

using Domain.Commands.Orders;

using MediatR;

namespace Application.CommandHandlers.LimitOrders;

public class DeleteLimitOrdersFromDatabaseCommandHandler : IRequestHandler<DeleteOrdersCommand, Unit>
{
    private readonly IFuturesOrdersRepository OrdersRepository;
    public DeleteLimitOrdersFromDatabaseCommandHandler(IFuturesOrdersRepository ordersRepository) => this.OrdersRepository = ordersRepository;

    public async Task<Unit> Handle(DeleteOrdersCommand request, CancellationToken cancellationToken)
    {
        await this.OrdersRepository.DeleteFuturesOrdersAsync(request.BybitIds);
        return Unit.Value;
    }
}
