using Application.Interfaces.Services.DataAccess;

using Domain.Models.Orders;

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

public class PositionNotificationHandler : INotificationHandler<PositionOpenedNotification>
{
    private readonly IFuturesPositionsRepository PositionsRepository;
    public PositionNotificationHandler(IFuturesPositionsRepository positionsRepository) => this.PositionsRepository = positionsRepository;

    public async Task Handle(PositionOpenedNotification notification, CancellationToken cancellationToken)
    {
        await this.PositionsRepository.AddFuturesPositionAsync(notification.Position, notification.FuturesOrders);
    }
}
