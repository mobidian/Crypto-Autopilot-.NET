using Application.Interfaces.Services;

using Domain.Models;

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

public class LimitOrderPlacedNotificationHandler : AbstractNotificationHandler<LimitOrderPlacedNotification>
{
    public LimitOrderPlacedNotificationHandler(IFuturesTradesDBService dbService) : base(dbService) { }

    public override async Task Handle(LimitOrderPlacedNotification notification, CancellationToken cancellationToken)
    {
        await this.DbService.AddFuturesOrderAsync(notification.LimitOrder);
    }
}
