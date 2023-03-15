using Bybit.Net.Enums;

using CryptoAutopilot.Api.Contracts.Responses.Common;

namespace CryptoAutopilot.Api.Contracts.Responses.Data.Market;

public class GetContractHistoryResponse
{
    public required string ContractName { get; init; }
    public required KlineInterval Timeframe { get; init; }
    public required IEnumerable<CandlestickResponse> Candlesticks { get; init; }
}
