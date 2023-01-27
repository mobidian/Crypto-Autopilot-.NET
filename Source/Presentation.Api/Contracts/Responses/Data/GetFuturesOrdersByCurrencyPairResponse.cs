using Binance.Net.Objects.Models.Futures;

namespace Presentation.Api.Contracts.Responses.Data;

public class GetFuturesOrdersByCurrencyPairResponse
{
    public required string CurrencyPair { get; init; }
    public required IEnumerable<BinanceFuturesOrder> FuturesOrders { get; init; }
}