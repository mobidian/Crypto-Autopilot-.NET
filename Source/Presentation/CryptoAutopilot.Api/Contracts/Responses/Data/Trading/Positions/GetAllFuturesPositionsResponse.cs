using CryptoAutopilot.Api.Contracts.Responses.Common;

namespace CryptoAutopilot.Api.Contracts.Responses.Data.Trading.Positions;

public class GetAllFuturesPositionsResponse
{
    public required IEnumerable<FuturesPositionResponse> FuturesPositions { get; init; }
}
