using Application.Interfaces.Services.DataAccess.Repositories;

using Domain.Commands.Orders;

using MediatR;

namespace Application.CommandHandlers.Orders;

public class CreateLimitOrderCommandHandler : IRequestHandler<CreateLimitOrderCommand, Unit>
{
    private readonly IFuturesOrdersRepository OrdersRepository;
    public CreateLimitOrderCommandHandler(IFuturesOrdersRepository ordersRepository) => this.OrdersRepository = ordersRepository;

    public async Task<Unit> Handle(CreateLimitOrderCommand request, CancellationToken cancellationToken)
    {
        await this.OrdersRepository.AddFuturesOrderAsync(request.LimitOrder);
        return Unit.Value;
    }
}
