using Domain.Models;

namespace CryptoAutopilot.Api.Contracts.Responses.Data;

public class GetAllFuturesOrdersResponse
{
    public required IEnumerable<FuturesOrder> FuturesOrders { get; init; }
}
