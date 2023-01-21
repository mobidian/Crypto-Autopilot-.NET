using Binance.Net.Objects.Models.Futures;

namespace Presentation.Api.Contracts.Responses;

public class GetAllFuturesOrdersResponse
{
    public required IEnumerable<BinanceFuturesOrder> FuturesOrders { get; init; }
}