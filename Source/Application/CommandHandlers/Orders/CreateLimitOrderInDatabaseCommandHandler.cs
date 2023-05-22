using Application.Interfaces.Services.DataAccess.Repositories;

using Domain.Commands.Orders;

using MediatR;

namespace Application.CommandHandlers.LimitOrders;

public class CreateLimitOrderInDatabaseCommandHandler : IRequestHandler<CreateLimitOrderCommand, Unit>
{
    private readonly IFuturesOrdersRepository OrdersRepository;
    public CreateLimitOrderInDatabaseCommandHandler(IFuturesOrdersRepository ordersRepository) => this.OrdersRepository = ordersRepository;
    
    public async Task<Unit> Handle(CreateLimitOrderCommand request, CancellationToken cancellationToken)
    {
        await this.OrdersRepository.AddFuturesOrderAsync(request.LimitOrder);
        return Unit.Value;
    }
}
