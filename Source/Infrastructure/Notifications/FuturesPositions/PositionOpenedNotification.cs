using Application.Interfaces.Services.DataAccess.Services;

using Domain.Models.Futures;

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
