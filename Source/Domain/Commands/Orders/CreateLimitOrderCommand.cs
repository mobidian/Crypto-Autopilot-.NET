using Domain.Models.Futures;

using MediatR;

namespace Domain.Commands.Orders;

/// <summary>
/// Represents a command that is sent when a limit order is placed.
/// </summary>
public class CreateLimitOrderCommand : IRequest<Unit>
{
    /// <summary>
    /// Gets or initializes the placed FuturesOrder as a limit order.
    /// </summary>
    public required FuturesOrder LimitOrder { get; init; }
}
