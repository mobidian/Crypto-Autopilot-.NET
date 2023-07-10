using Domain.Models.Futures;

namespace Application.DataAccess.Services;

public interface IFuturesOperationsService
{
    /// <summary>
    /// Adds a <see cref="FuturesPosition"/> object along with the associated <see cref="FuturesOrder"/> objects to the database as a single database transaction.
    /// </summary>
    /// <param name="position">The <see cref="FuturesPosition"/> object to be added.</param>
    /// <param name="orders">The associated <see cref="FuturesOrder"/> objects to be added.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task AddFuturesPositionAndOrdersAsync(FuturesPosition position, IEnumerable<FuturesOrder> orders);

    /// <summary>
    /// Updates an existing <see cref="FuturesPosition"/> object using its unique <see cref="FuturesPosition.CryptoAutopilotId"/> and adds new associated <see cref="FuturesOrder"/> objects to the database as a single database transaction.
    /// </summary>
    /// <param name="updatedPosition">The <see cref="FuturesPosition"/> object to be updated.</param>
    /// <param name="newOrders">The new <see cref="FuturesOrder"/> objects to be added.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task UpdateFuturesPositionAndAddOrdersAsync(FuturesPosition updatedPosition, IEnumerable<FuturesOrder> newOrders);

    /// <summary>
    /// <para>Updates multiple <see cref="FuturesPosition"/> objects, identified by their unique <see cref="FuturesPosition.CryptoAutopilotId"/>, and their related <see cref="FuturesOrder"/> objects in the database.</para> 
    /// <para>All updates and additions of positions and their related orders are performed in a single database transaction.</para>
    /// </summary>
    /// <param name="positionsOrders">A Dictionary containing the <see cref="FuturesPosition"/> objects as keys and their associated <see cref="IEnumerable{FuturesOrder}"/> objects as values.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task UpdateFuturesPositionsAndAddTheirOrdersAsync(Dictionary<FuturesPosition, IEnumerable<FuturesOrder>> positionsOrders);
}
