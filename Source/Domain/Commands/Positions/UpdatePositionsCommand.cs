using MediatR;

namespace Domain.Commands.Positions;

/// <summary>
/// Represents a command that is sent when multiple existing positions are updated.
/// </summary>
public class UpdatePositionsCommand : IRequest<Unit>
{
    /// <summary>
    /// Gets or initializes the commands that will be executed to update the positions.
    /// </summary>
    public IEnumerable<UpdatePositionCommand> Commands { get; init; } = Enumerable.Empty<UpdatePositionCommand>();
}
