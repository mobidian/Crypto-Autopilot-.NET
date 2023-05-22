using MediatR;

namespace Domain.Commands.Orders;

/// <summary>
/// <para>Represents a command that is sent when an order needs to be deleted.</para>
/// <para>An example would be when an existing limit order is canceled.</para>
/// </summary>
public class DeleteOrdersCommand : IRequest<Unit>
{
    /// <summary>
    /// Gets or initializes the unique identifier given by Bybit of the Bybit orders.
    /// </summary>
    public required Guid[] BybitIds { get; init; }
}
