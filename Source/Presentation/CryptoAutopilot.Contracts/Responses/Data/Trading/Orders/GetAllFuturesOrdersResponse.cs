using CryptoAutopilot.Contracts.Responses.Common;

namespace CryptoAutopilot.Contracts.Responses.Data.Trading.Orders;

public class GetAllFuturesOrdersResponse
{
    public required IEnumerable<FuturesOrderResponse> FuturesOrders { get; init; }
}
