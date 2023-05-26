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
    public required Guid[] BybitIds { get; init; }
}
