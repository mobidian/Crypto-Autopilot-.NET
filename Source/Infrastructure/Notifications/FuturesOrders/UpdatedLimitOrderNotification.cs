using Application.Interfaces.Services.DataAccess;

using Domain.Models;

using MediatR;

namespace Infrastructure.Notifications.FuturesOrders;

/// <summary>
/// Represents a notification that is published when an existing limit order is updated in the.
/// </summary>
public class UpdatedLimitOrderNotification : INotification
{
    /// <summary>
    /// Gets or initializes the unique identifier given by Bybit of the limit order being updated.
    /// </summary>
    public required Guid BybitId { get; init; }

    /// <summary>
    /// Gets or initializes the updated FuturesOrder as a limit order.
    /// </summary>
    public required FuturesOrder UpdatedLimitOrder { get; init; }
}

public class UpdatedLimitOrderNotificationHandler : AbstractNotificationHandler<UpdatedLimitOrderNotification>
{
    public UpdatedLimitOrderNotificationHandler(IFuturesTradesDBService dbService) : base(dbService) { }

    public override async Task Handle(UpdatedLimitOrderNotification notification, CancellationToken cancellationToken)
    {
        await this.DbService.UpdateFuturesOrderAsync(notification.BybitId, notification.UpdatedLimitOrder);
    }
}
