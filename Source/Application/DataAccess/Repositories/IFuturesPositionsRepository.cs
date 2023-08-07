using Domain.Models.Futures;

namespace Application.DataAccess.Repositories;

/// <summary>
/// Represents a repository for futures positions.
/// </summary>
public interface IFuturesPositionsRepository
{
    /// <summary>
    /// Adds a single futures position asynchronously.
    /// </summary>
    /// <param name="position">The futures position to add.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task AddAsync(FuturesPosition position);

    /// <summary>
    /// Adds multiple futures positions asynchronously.
    /// </summary>
    /// <param name="positions">The futures positions to add.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task AddAsync(IEnumerable<FuturesPosition> positions);


    /// <summary>
    /// Gets a futures position by its CryptoAutopilotId asynchronously.
    /// </summary>
    /// <param name="cryptoAutopilotId">The ID of the futures position to get.</param>
    /// <returns>A task that returns the futures position.</returns>
    public Task<FuturesPosition?> GetByCryptoAutopilotId(Guid cryptoAutopilotId);

    /// <summary>
    /// Gets all futures positions asynchronously.
    /// </summary>
    /// <returns>A task that returns all futures positions.</returns>
    public Task<IEnumerable<FuturesPosition>> GetAllAsync();

    /// <summary>
    /// Gets futures positions by currency pair asynchronously.
    /// </summary>
    /// <param name="currencyPair">The currency pair to get futures positions for.</param>
    /// <returns>A task that returns the futures positions.</returns>
    public Task<IEnumerable<FuturesPosition>> GetByCurrencyPairAsync(string currencyPair);


    /// <summary>
    /// Updates a futures position asynchronously.
    /// </summary>
    /// <param name="updatedPosition">The futures position to update.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task UpdateAsync(FuturesPosition updatedPosition);
}
