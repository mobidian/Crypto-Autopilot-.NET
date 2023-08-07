using Domain.Models.Futures;

namespace Application.DataAccess.Repositories;

/// <summary>
/// Represents a repository for futures orders.
/// </summary>
public interface IFuturesOrdersRepository
{
    /// <summary>
    /// Adds a single futures order asynchronously.
    /// </summary>
    /// <param name="futuresOrder">The futures order to add.</param>
    /// <param name="positionId">Optional position ID to associate with the futures order.</param>
    /// <returns>A task that returns <see langword="true"/> if the operation succeeded, <see langword="false"/> otherwise.</returns>
    public Task<bool> AddAsync(FuturesOrder futuresOrder, Guid? positionId = null);

    /// <summary>
    /// Adds multiple futures orders asynchronously.
    /// </summary>
    /// <param name="futuresOrder">The futures orders to add.</param>
    /// <param name="positionId">Optional position ID to associate with the futures orders.</param>
    /// <returns>A task that returns <see langword="true"/> if the operation succeeded, <see langword="false"/> otherwise.</returns>
    public Task<bool> AddAsync(IEnumerable<FuturesOrder> futuresOrder, Guid? positionId = null);


    /// <summary>
    /// Gets a futures order by its BybitID asynchronously.
    /// </summary>
    /// <param name="bybitID">The BybitID of the futures order to get.</param>
    /// <returns>A task that returns the futures order.</returns>
    public Task<FuturesOrder?> GetByBybitId(Guid bybitID);

    /// <summary>
    /// Gets all futures orders asynchronously.
    /// </summary>
    /// <returns>A task that returns all futures orders.</returns>
    public Task<IEnumerable<FuturesOrder>> GetAllAsync();

    /// <summary>
    /// Gets futures orders by currency pair asynchronously.
    /// </summary>
    /// <param name="currencyPair">The currency pair to get futures orders for.</param>
    /// <returns>A task that returns the futures orders.</returns>
    public Task<IEnumerable<FuturesOrder>> GetByCurrencyPairAsync(string currencyPair);


    /// <summary>
    /// Updates a futures order asynchronously.
    /// </summary>
    /// <param name="updatedFuturesOrder">The futures order to update.</param>
    /// <param name="positionId">Optional position ID to associate with the futures order.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task UpdateAsync(FuturesOrder updatedFuturesOrder, Guid? positionId = null);


    /// <summary>
    /// Deletes futures orders with the specified BybitIDs asynchronously.
    /// </summary>
    /// <param name="bybitIDs">The BybitIDs of the futures orders to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task DeleteAsync(params Guid[] bybitIDs);
}
