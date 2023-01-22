namespace Presentation.Api.Contracts.Responses;

public class StrategyEngineStartedResponse
{
    public required Guid Guid { get; init; }
    public required string StartedStrategyTypeName { get; init; }
    public required DateTime StartTime { get; init; }
}
