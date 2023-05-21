using Application.Interfaces.Services.DataAccess.Services;

using Domain.Models.Futures;

using MediatR;

namespace Infrastructure.Notifications.FuturesPositions;

/// <summary>
/// Represents a notification that is published when an existing position is updated.
/// </summary>
public class PositionUpdatedNotification : INotification
{
    /// <summary>
    /// Gets or initializes the updated FuturesPosition.
    /// </summary>
    public required FuturesPosition UpdatedPosition { get; init; }

    /// <summary>
    /// <para>Gets or initializes the collection of related FuturesOrders for the updated position.</para>
    /// <para>These orders do not include the previous orders related to the FuturesOrders, just the new ones which caused the position to be updated.</para>
    /// </summary>
    public IEnumerable<FuturesOrder> NewFuturesOrders { get; init; } = Enumerable.Empty<FuturesOrder>();
}

public class PositionUpdatedNotificationHandler : INotificationHandler<PositionUpdatedNotification>
{
    private readonly IFuturesOperationsService FuturesOperationsService;

    public PositionUpdatedNotificationHandler(IFuturesOperationsService futuresOperationsService)
    {
        this.FuturesOperationsService = futuresOperationsService;
    }
    
    
    public async Task Handle(PositionUpdatedNotification notification, CancellationToken cancellationToken)
    {
        var cryptoAutopilotId = notification.UpdatedPosition.CryptoAutopilotId;
        var updatedPosition = notification.UpdatedPosition;
        var newFuturesOrders = notification.NewFuturesOrders;

        await this.FuturesOperationsService.UpdateFuturesPositionAndAddOrdersAsync(cryptoAutopilotId, updatedPosition, newFuturesOrders);
    }
}
