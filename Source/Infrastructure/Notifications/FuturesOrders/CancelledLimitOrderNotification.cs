using Application.Interfaces.Services;

using MediatR;

namespace Infrastructure.Notifications.FuturesOrders;

/// <summary>
/// Represents a notification that is published when a limit order is canceled in the trading service.
/// </summary>
public class CancelledLimitOrderNotification : INotification
{
    /// <summary>
    /// Gets or initializes the unique identifier given by Bybit of the canceled Bybit limit order.
    /// </summary>
    public required Guid BybitId { get; init; }
}

public class CancelledLimitOrderNotificationHandler : AbstractNotificationHandler<CancelledLimitOrderNotification>
{
    public CancelledLimitOrderNotificationHandler(IFuturesTradesDBService dbService) : base(dbService) { }

    public override async Task Handle(CancelledLimitOrderNotification notification, CancellationToken cancellationToken)
    {
        await this.DbService.DeleteFuturesOrderAsync(notification.BybitId);
    }
}
