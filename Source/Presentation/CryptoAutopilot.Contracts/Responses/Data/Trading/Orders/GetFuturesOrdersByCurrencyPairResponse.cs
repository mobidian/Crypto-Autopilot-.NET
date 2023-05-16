using CryptoAutopilot.Contracts.Responses.Common;

namespace CryptoAutopilot.Contracts.Responses.Data.Trading.Orders;

public class GetFuturesOrdersByContractNameResponse
{
    public required string ContractName { get; init; }
    public required IEnumerable<FuturesOrderResponse> FuturesOrders { get; init; }
}
