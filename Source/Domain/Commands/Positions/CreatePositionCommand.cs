using Domain.Models.Futures;

using MediatR;

namespace Domain.Commands.Positions;

/// <summary>
/// Represents a command that is sent when a new position is opened.
/// </summary>
public class CreatePositionCommand : IRequest<Unit>
{
    /// <summary>
    /// Gets or initializes the opened FuturesPosition.
    /// </summary>
    public required FuturesPosition Position { get; init; }

    /// <summary>
    /// Gets or initializes the collection of related FuturesOrders for the opened position.
    /// </summary>
    public required IEnumerable<FuturesOrder> FuturesOrders { get; init; }
}
