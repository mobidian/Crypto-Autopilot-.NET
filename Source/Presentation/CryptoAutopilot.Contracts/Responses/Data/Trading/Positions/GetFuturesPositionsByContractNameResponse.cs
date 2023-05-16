using CryptoAutopilot.Contracts.Responses.Common;

namespace CryptoAutopilot.Contracts.Responses.Data.Trading.Positions;

public class GetFuturesPositionsByContractNameResponse
{
    public required string ContractName { get; init; }
    public required IEnumerable<FuturesPositionResponse> FuturesPositions { get; init; }
}
