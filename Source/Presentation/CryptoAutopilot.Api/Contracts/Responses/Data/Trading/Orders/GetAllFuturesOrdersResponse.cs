using CryptoAutopilot.Api.Contracts.Responses.Common;

namespace CryptoAutopilot.Api.Contracts.Responses.Data.Trading.Orders;

public class GetAllFuturesOrdersResponse
{
    public required IEnumerable<FuturesOrderResponse> FuturesOrders { get; init; }
}
