using Domain.Models;

namespace Presentation.Api.Contracts.Responses.Data;

public class GetCandlesticksByCurrencyPairResponse
{
    public required string CurrencyPair { get; init; }
    public required IEnumerable<Candlestick> Candlesticks { get; init; }
}
