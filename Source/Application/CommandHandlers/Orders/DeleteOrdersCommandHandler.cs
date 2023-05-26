using Application.DataAccess.Repositories;

using Domain.Commands.Orders;

using MediatR;

namespace Application.CommandHandlers.Orders;

public class DeleteOrdersCommandHandler : IRequestHandler<DeleteOrdersCommand, Unit>
{
    private readonly IFuturesOrdersRepository OrdersRepository;
    public DeleteOrdersCommandHandler(IFuturesOrdersRepository ordersRepository) => this.OrdersRepository = ordersRepository;

    public async Task<Unit> Handle(DeleteOrdersCommand request, CancellationToken cancellationToken)
    {
        await this.OrdersRepository.DeleteFuturesOrdersAsync(request.BybitIds);
        return Unit.Value;
    }
}
