using Domain.Models;

namespace CryptoAutopilot.Api.Contracts.Responses.Data;

public class GetAllCandlesticksResponse
{
    public required IEnumerable<Candlestick> Candlesticks { get; init; }
}
