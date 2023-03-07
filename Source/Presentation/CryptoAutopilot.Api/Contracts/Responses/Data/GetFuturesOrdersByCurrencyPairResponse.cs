using Domain.Models;

namespace CryptoAutopilot.Api.Contracts.Responses.Data;

public class GetFuturesOrdersByCurrencyPairResponse
{
    public required string CurrencyPair { get; init; }
    public required IEnumerable<FuturesOrder> FuturesOrders { get; init; }
}