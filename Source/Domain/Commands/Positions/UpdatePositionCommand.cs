using Domain.Models.Futures;

using MediatR;

namespace Domain.Commands.Positions;

/// <summary>
/// Represents a command that is sent when an existing position is updated.
/// </summary>
public class UpdatePositionCommand : IRequest<Unit>
{
    /// <summary>
    /// Gets or initializes the updated FuturesPosition.
    /// </summary>
    public required FuturesPosition UpdatedPosition { get; init; }

    /// <summary>
    /// <para>Gets or initializes the collection of related FuturesOrders for the updated position.</para>
    /// <para>These orders do not include the previous orders related to the FuturesOrders, just the new ones which caused the position to be updated.</para>
    /// </summary>
    public IEnumerable<FuturesOrder> NewFuturesOrders { get; init; } = Enumerable.Empty<FuturesOrder>();
}
