using Application.Interfaces.Services;

using Domain.Models;

using MediatR;

using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Notifications.FuturesPositions;

/// <summary>
/// Represents a notification that is published when an existing position is updated.
/// </summary>
public class PositionUpdatedNotification : INotification
{
    /// <summary>
    /// Gets or initializes the unique identifier given by CryptoAutopilot of the position being updated.
    /// </summary>
    public required Guid PositionCryptoAutopilotId { get; init; }

    /// <summary>
    /// Gets or initializes the updated FuturesPosition.
    /// </summary>
    public required FuturesPosition UpdatedPosition { get; init; }

    /// <summary>
    /// Gets or initializes the collection of related FuturesOrders for the updated position.
    /// </summary>
    public IEnumerable<FuturesOrder> FuturesOrders { get; init; } = Enumerable.Empty<FuturesOrder>();
}

public class PositionUpdatedNotificationHandler : AbstractNotificationHandler<PositionUpdatedNotification>
{
    public PositionUpdatedNotificationHandler(IFuturesTradesDBService dbService) : base(dbService) { }

    public override async Task Handle(PositionUpdatedNotification notification, CancellationToken cancellationToken)
    {
        if (!notification.FuturesOrders.IsNullOrEmpty())
            await this.DbService.AddFuturesOrdersAsync(notification.FuturesOrders, notification.PositionCryptoAutopilotId);

        await this.DbService.UpdateFuturesPositionAsync(notification.PositionCryptoAutopilotId, notification.UpdatedPosition);
    }
}
