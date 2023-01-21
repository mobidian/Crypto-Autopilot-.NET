using Domain.Models;

namespace Presentation.Api.Contracts.Responses;

public class GetAllCandlesticksResponse
{
    public required IEnumerable<Candlestick> Candlesticks { get; init; }
}
