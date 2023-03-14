using CryptoAutopilot.Api.Contracts.Responses.Common;

namespace CryptoAutopilot.Api.Contracts.Responses.Data;

public class GetFuturesOrdersByCurrencyPairResponse
{
    public required string CurrencyPair { get; init; }
    public required IEnumerable<FuturesOrderResponse> FuturesOrders { get; init; }
}