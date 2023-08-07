using Domain.Models.Signals;

namespace Application.DataAccess.Repositories;

/// <summary>
/// Represents a repository for trading signals.
/// </summary>
public interface ITradingSignalsRepository
{
    /// <summary>
    /// Adds a single trading signal asynchronously.
    /// </summary>
    /// <param name="tradingSignal">The trading signal to add.</param>
    /// <returns>A task that returns true if the operation succeeded, false otherwise.</returns>
    public Task<bool> AddAsync(TradingSignal tradingSignal);

    /// <summary>
    /// Adds multiple trading signals asynchronously.
    /// </summary>
    /// <param name="tradingSignals">The trading signals to add.</param>
    /// <returns>A task that returns true if the operation succeeded, false otherwise.</returns>
    public Task<bool> AddAsync(IEnumerable<TradingSignal> tradingSignals);


    /// <summary>
    /// Gets a trading signal by its CryptoAutopilotId asynchronously.
    /// </summary>
    /// <param name="cryptoAutopilotId">The CryptoAutopilotId of the trading signal to get.</param>
    /// <returns>A task that returns the trading signal.</returns>
    public Task<TradingSignal?> GetByCryptoAutopilotIdAsync(Guid cryptoAutopilotId);

    /// <summary>
    /// Gets all trading signals associated with a specific contract asynchronously.
    /// </summary>
    /// <param name="contract">The contract to filter trading signals by.</param>
    /// <returns>A task that returns the trading signals.</returns>
    public Task<IEnumerable<TradingSignal>> GetAllWithContractAsync(string contract);

    /// <summary>
    /// Gets all trading signals asynchronously.
    /// </summary>
    /// <returns>A task that returns all trading signals.</returns>
    public Task<IEnumerable<TradingSignal>> GetAllAsync();


    /// <summary>
    /// Updates a trading signal asynchronously.
    /// </summary>
    /// <param name="updatedSignal">The trading signal to update.</param>
    /// <returns>A task that returns true if the operation succeeded, false otherwise.</returns>
    public Task<bool> UpdateAsync(TradingSignal updatedSignal);


    /// <summary>
    /// Deletes the trading signals with the specified CryptoAutopilotIds asynchronously.
    /// </summary>
    /// <param name="cryptoAutopilotIds">The CryptoAutopilotIds of the trading signals to delete.</param>
    /// <returns>A task that returns true if the operation succeeded, false otherwise.</returns>
    public Task<bool> DeleteAsync(params Guid[] cryptoAutopilotIds);
}
