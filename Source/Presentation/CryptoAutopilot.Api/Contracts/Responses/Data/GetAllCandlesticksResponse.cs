using Domain.Models;

namespace Presentation.Api.Contracts.Responses.Data;

public class GetAllCandlesticksResponse
{
    public required IEnumerable<Candlestick> Candlesticks { get; init; }
}
