using Bybit.Net.Enums;

namespace CryptoAutopilot.Contracts.Responses.Common;

public class FuturesPositionResponse
{
    public required Guid CryptoAutopilotId { get; init; }
    public required string ContractName { get; init; }
    public required PositionSide Side { get; init; }
    public required decimal Margin { get; init; }
    public required decimal Leverage { get; init; }
    public required decimal Quantity { get; init; }
    public required decimal EntryPrice { get; init; }
    public required decimal? ExitPrice { get; set; }
}
