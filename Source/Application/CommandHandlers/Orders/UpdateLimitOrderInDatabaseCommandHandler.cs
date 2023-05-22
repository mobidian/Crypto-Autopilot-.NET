﻿using Application.Interfaces.Services.DataAccess.Repositories;

using Domain.Commands.Orders;

using MediatR;

namespace Application.CommandHandlers.LimitOrders;

public class UpdateLimitOrderInDatabaseCommandHandler : IRequestHandler<UpdateOrderCommand, Unit>
{
    private readonly IFuturesOrdersRepository OrdersRepository;
    public UpdateLimitOrderInDatabaseCommandHandler(IFuturesOrdersRepository ordersRepository) => this.OrdersRepository = ordersRepository;

    public async Task<Unit> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var bybitId = request.UpdatedOrder.BybitID;
        var updatedLimitOrder = request.UpdatedOrder;

        await this.OrdersRepository.UpdateFuturesOrderAsync(bybitId, updatedLimitOrder);
        return Unit.Value;
    }
}
