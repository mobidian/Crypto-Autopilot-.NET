using Application.Exceptions;

using Binance.Net.Enums;

namespace Application.Interfaces.Services.Trading.Binance.Monitors;

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
    /// <para>Waits for an order to reach a specific status asynchronously</para>
    /// <para>Commonly used with OTO and OTOCO orders</para>
    /// </summary>
    /// <param name="OrderID">The ID of the order to wait for.</param>
    /// <param name="OrderStatus">The status to wait for</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="NotSubscribedException"></exception>
    /// <exception cref="TaskCanceledException"></exception>
    public Task WaitForOrderToReachStatusAsync(long OrderID, OrderStatus OrderStatus, CancellationToken token = default);

    /// <summary>
    /// <para>Waits for any of the orders with the specified IDs to reach the specified status asynchronously</para>
    /// <para>Commonly used with OCO orders</para>
    /// </summary>
    /// <param name="OrderIDs">The IDs of the orders</param>
    /// <param name="OrderStatus">The status to wait for</param>
    /// <returns>
    /// <para>A task representing the asynchronous operation</para>
    /// <para>The task result is the ID of the first order that was found with the specified status</para>
    /// <para>If you are expecting multiple orders to reach the specified status at rougly the same time it is recommended to check the status of each order yourself after the execution of this method finishes</para>
    /// </returns>
    /// <exception cref="NotSubscribedException"></exception>
    /// <exception cref="TaskCanceledException"></exception>
    public Task<long> WaitForAnyOrderToReachStatusAsync(IEnumerable<long> OrderIDs, OrderStatus OrderStatus, CancellationToken token = default);
}
