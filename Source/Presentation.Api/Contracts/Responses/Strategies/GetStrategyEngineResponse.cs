namespace Presentation.Api.Contracts.Responses.Strategies;

public class GetStrategyEngineResponse
{
    public required Guid Guid { get; init; }
    public required string StartedStrategyTypeName { get; init; }
    public required bool IsRunning { get; init; }
}
