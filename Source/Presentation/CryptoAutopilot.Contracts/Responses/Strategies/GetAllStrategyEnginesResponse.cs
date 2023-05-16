namespace CryptoAutopilot.Contracts.Responses.Strategies;

public class GetAllStrategyEnginesResponse
{
    public required IEnumerable<StrategyEngineResponse> Strategies { get; init; }
}
