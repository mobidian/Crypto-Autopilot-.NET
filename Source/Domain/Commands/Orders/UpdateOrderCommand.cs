using Domain.Models.Futures;

using MediatR;

namespace Domain.Commands.Orders;

/// <summary>
/// Represents a command that is sent when an existing order is updated.
/// </summary>
public class UpdateOrderCommand : IRequest<Unit>
{
    /// <summary>
    /// Gets or initializes the updated FuturesOrder.
    /// </summary>
    public required FuturesOrder UpdatedOrder { get; init; }

    /// <summary>
    /// Gets or initializes the CryptoAutopilotId of the position related to the UpdatedOrder.
    /// </summary>
    public Guid? FuturesPositionId { get; init; }
}
