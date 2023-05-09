using Application.Interfaces.Services.DataAccess;

using Domain.Models;

using MediatR;

namespace Infrastructure.Notifications.FuturesPositions;

/// <summary>
/// Represents a notification that is published when a new position is opened.
/// </summary>
public class PositionOpenedNotification : INotification
{
    /// <summary>
    /// Gets or initializes the opened FuturesPosition.
    /// </summary>
    public required FuturesPosition Position { get; init; }

    /// <summary>
    /// Gets or initializes the collection of related FuturesOrders for the opened position.
    /// </summary>
    public required IEnumerable<FuturesOrder> FuturesOrders { get; init; }
}

public class PositionNotificationHandler : AbstractNotificationHandler<PositionOpenedNotification>
{
    public PositionNotificationHandler(IFuturesTradesDBService dbService) : base(dbService) { }

    public override async Task Handle(PositionOpenedNotification notification, CancellationToken cancellationToken)
    {
        await this.DbService.AddFuturesPositionAsync(notification.Position, notification.FuturesOrders);
    }
}
