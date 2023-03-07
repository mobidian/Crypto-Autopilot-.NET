using Application.Exceptions;

using Bybit.Net.Enums;

namespace Application.Interfaces.Services.Trading.Bybit.Monitors;

/// <summary>
/// Defines an interface for monitoring the status of ByBit USD perpetual orders
/// </summary>
public interface IByBitUsdPerpetualOrderMonitor
{
    /// <summary>
    /// Gets a value indicating whether this order monitor is currently subscribed to order updates
    /// </summary>
    public bool Subscribed { get; }

    
    /// <summary>
    /// Subscribes to order updates asynchronously
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public Task SubscribeToOrderUpdatesAsync();

    /// <summary>
    /// Unsubscribes from order updates asynchronously
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public Task UnsubscribeFromOrderUpdatesAsync();

    /// <summary>
    /// <para>Waits for the specified order to reach the specified status</para>
    /// <para>If the order with the specified ID doesn't reach the specified status, the task will complete when cancellation is requested or the consumer unsubscribes from order updates</para>
    /// <para>If the consumer unsubscribes a <see cref="NotSubscribedException"/> will be thrown</para>
    /// <para>If cancellation is requested a <see cref="TaskCanceledException"/> will be thrown</para>
    /// </summary>
    /// <param name="orderID">The ID of the order to monitor</param>
    /// <param name="orderStatus">The status of the order to wait for</param>
    /// <param name="token">A cancellation token that can be used to cancel the operation</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="NotSubscribedException"></exception>
    /// <exception cref="TaskCanceledException"></exception>
    public Task WaitForOrderToReachStatusAsync(Guid orderID, OrderStatus orderStatus, CancellationToken token = default);
    
    /// <summary>
    /// <para>Waits for any of the specified orders to reach the specified status and returns the ID of the first order that reaches the specified status</para>
    /// <para>If none of the specified orders reach the specified status, the task will complete when cancellation is requested or the consumer unsubscribes from order updates</para>
    /// <para>If the consumer unsubscribes a <see cref="NotSubscribedException"/> will be thrown</para>
    /// <para>If cancellation is requested a <see cref="TaskCanceledException"/> will be thrown</para>
    /// </summary>
    /// <param name="orderIDs">The IDs of the orders to monitor</param>
    /// <param name="orderStatus">The status of the order to wait for</param>
    /// <param name="token">A cancellation token that can be used to cancel the operation</param>
    /// <returns>
    /// <para>A task that represents the asynchronous operation</para>
    /// <para>The task result is the ID of the first order that reaches the specified status</para>
    /// </returns>
    /// <exception cref="NotSubscribedException"></exception>
    /// <exception cref="TaskCanceledException"></exception>
    public Task<Guid> WaitForAnyOrderToReachStatusAsync(IEnumerable<Guid> orderIDs, OrderStatus orderStatus, CancellationToken token = default);
}
