using Application.Interfaces.Services.DataAccess.Repositories;

using MediatR;

namespace Infrastructure.Notifications.FuturesOrders;

/// <summary>
/// Represents a notification that is published when a limit order is canceled in the trading service.
/// </summary>
public class CancelledLimitOrdersNotification : INotification
{
    /// <summary>
    /// Gets or initializes the unique identifier given by Bybit of the canceled Bybit limit order.
    /// </summary>
    public required IEnumerable<Guid> BybitIds { get; init; }
}

public class CancelledLimitOrdersNotificationHandler : INotificationHandler<CancelledLimitOrdersNotification>
{
    private readonly IFuturesOrdersRepository OrdersRepository;
    public CancelledLimitOrdersNotificationHandler(IFuturesOrdersRepository ordersRepository) => this.OrdersRepository = ordersRepository;

    public async Task Handle(CancelledLimitOrdersNotification notification, CancellationToken cancellationToken)
    {
        await this.OrdersRepository.DeleteFuturesOrdersAsync(notification.BybitIds.ToArray());
    }
}
