using CryptoAutopilot.Contracts.Responses.Common;

namespace CryptoAutopilot.Contracts.Responses.Data.Trading.Positions;

public class GetAllFuturesPositionsResponse
{
    public required IEnumerable<FuturesPositionResponse> FuturesPositions { get; init; }
}
