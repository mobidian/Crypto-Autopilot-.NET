using Application.Interfaces.Services.Trading.Strategy;

namespace CryptoAutopilot.Api.Contracts.Responses.Strategies;

public class GetAllStrategyEnginesResponse
{
    public required IEnumerable<GetStrategyEngineResponse> Strategies { get; init; }
}
