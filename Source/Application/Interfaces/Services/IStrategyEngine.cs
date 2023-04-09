namespace Application.Interfaces.Services;

/// <summary>
/// <para>The IStrategyEngine interface defines the structure and lifecycle management methods for a trading strategy.</para>
/// <para>Implementations of this interface should provide the necessary logic for starting, running, and stopping a trading strategy.</para>
/// </summary>
public interface IStrategyEngine : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets the unique identifier for the trading strategy instance.
    /// </summary>
    public Guid Guid { get; }

    /// <summary>
    /// Starts the trading strategy and keeps it running until cancellation is requested.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public Task StartTradingAsync();

    /// <summary>
    /// Checks if the trading strategy is currently running.
    /// </summary>
    /// <returns>True if the strategy is running, otherwise false.</returns>
    public bool IsRunning();

    /// <summary>
    /// Stops the trading strategy and waits for it to complete its current action.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public Task StopTradingAsync();
}
