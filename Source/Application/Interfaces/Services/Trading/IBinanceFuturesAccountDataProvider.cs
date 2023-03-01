using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;

namespace Application.Interfaces.Services.Trading;

/// <summary>
/// Provides data from a Binance USDⓈ-M Futures markets account
/// </summary>
public interface IBinanceFuturesAccountDataProvider
{
    public Task<decimal> GetEquityAsync(string currencyPair);
    
    public Task<IEnumerable<BinancePositionDetailsUsdt>> GetPositionsAsync(string currencyPair);
    public Task<IEnumerable<BinancePositionDetailsUsdt>> GetPositionAsync(string currencyPair, PositionSide positionSide);
}
