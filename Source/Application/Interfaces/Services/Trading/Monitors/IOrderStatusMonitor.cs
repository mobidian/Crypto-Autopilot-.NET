using Binance.Net.Enums;

namespace Application.Interfaces.Services.Trading.Monitors;

/// <summary>
/// Interface for monitoring the status of futures markets orders
/// </summary>
public interface IOrderStatusMonitor
{
    /// <summary>
    /// Gets a value indicating whether the monitor is currently subscribed to order updates
    /// </summary>
    public bool Subscribed { get; }

    /// <summary>
    /// Subscribes to order updates asynchronously
    /// </summary>
    public Task SubscribeToOrderUpdatesAsync();

    /// <summary>
    /// Unsubscribes from order updates asynchronously.
    /// </summary>
    public Task UnsubscribeFromOrderUpdatesAsync();

    /// <summary>
    /// Gets the status of an order asynchronously
    /// </summary>
    /// <param name="OrderID">The ID of the order to get the status of</param>
    /// <exception cref="KeyNotFoundException"></exception>
    public ValueTask<OrderStatus> GetStatusAsync(long OrderID);

    /// <summary>
    /// Waits for an order to reach a specific status asynchronously.
    /// </summary>
    /// <param name="OrderID">The ID of the order to wait for.</param>
    /// <param name="OrderStatus">The status to wait for</param>
    public Task WaitForOrderStatusAsync(long OrderID, OrderStatus OrderStatus);
}

