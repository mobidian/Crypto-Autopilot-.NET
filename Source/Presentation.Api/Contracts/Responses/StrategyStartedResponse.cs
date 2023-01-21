namespace Presentation.Api.Contracts.Responses;

public class StrategyStartedResponse
{
    public required Guid Guid { get; init; }
    public required string StrategyTypeName { get; init; }
    public required DateTime Timestamp { get; init; }
}
