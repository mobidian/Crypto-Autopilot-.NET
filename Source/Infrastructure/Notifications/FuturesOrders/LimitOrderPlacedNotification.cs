using Application.Interfaces.Services.DataAccess.Repositories;

using Domain.Models.Orders;

using MediatR;

namespace Infrastructure.Notifications.FuturesOrders;

/// <summary>
/// Represents a notification that is published when a limit order is placed.
/// </summary>
public class LimitOrderPlacedNotification : INotification
{
    /// <summary>
    /// Gets or initializes the placed FuturesOrder as a limit order.
    /// </summary>
    public required FuturesOrder LimitOrder { get; init; }
}

public class LimitOrderPlacedNotificationHandler : INotificationHandler<LimitOrderPlacedNotification>
{
    private readonly IFuturesOrdersRepository OrdersRepository;
    public LimitOrderPlacedNotificationHandler(IFuturesOrdersRepository ordersRepository) => this.OrdersRepository = ordersRepository;

    public async Task Handle(LimitOrderPlacedNotification notification, CancellationToken cancellationToken)
    {
        await this.OrdersRepository.AddFuturesOrderAsync(notification.LimitOrder);
    }
}
