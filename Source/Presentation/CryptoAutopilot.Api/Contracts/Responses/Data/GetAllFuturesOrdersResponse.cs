using Binance.Net.Objects.Models.Futures;

namespace Presentation.Api.Contracts.Responses.Data;

public class GetAllFuturesOrdersResponse
{
    public required IEnumerable<BinanceFuturesOrder> FuturesOrders { get; init; }
}
