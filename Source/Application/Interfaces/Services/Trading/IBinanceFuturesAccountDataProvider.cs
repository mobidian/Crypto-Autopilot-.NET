namespace Application.Interfaces.Services.Trading;

public interface IBinanceFuturesAccountDataProvider
{
    public Task<decimal> GetEquityAsync(string currencyPair);
}
