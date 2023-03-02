using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;

namespace Application.Interfaces.Services.Trading;

/// <summary>
/// Client for interacting with the Binance USDⓈ-M Futures markets
/// </summary>
public interface IBinanceFuturesApiService
{
    /// <summary>
    /// Opens a position at the current market price
    /// </summary>
    /// <param name="currencyPair">The name of the cryptocurrency contract to trade</param>
    /// <param name="orderSide">The side of the order</param>
    /// <param name="Margin">The margin for the position</param>
    /// <param name="Leverage">The leverage to use for the position</param>
    /// <param name="StopLoss">The stop loss price</param>
    /// <param name="TakeProfit">The take profit price</param>
    /// <returns>The position that has been opened</returns>
    public Task<IEnumerable<BinanceFuturesOrder>> OpenPositionAsync(string currencyPair, OrderSide orderSide, decimal Margin, decimal Leverage, decimal? StopLoss = null, decimal? TakeProfit = null);
    
    /// <summary>
    /// Places a limit order and the position will open when the limit order is filled
    /// </summary>
    /// <param name="currencyPair">The name of the cryptocurrency contract to trade</param>
    /// <param name="orderSide">The side of the order</param>
    /// <param name="LimitPrice">The limit price for the order</param>
    /// <param name="Margin">The margin for the position</param>
    /// <param name="Leverage">The leverage to use for the position</param>
    /// <param name="StopLoss">The stop loss price</param>
    /// <param name="TakeProfit">The take profit price</param>
    /// <returns>The order that has been placed</returns>
    public Task<BinanceFuturesOrder> PlaceLimitOrderAsync(string currencyPair, OrderSide orderSide, decimal LimitPrice, decimal Margin, decimal Leverage, decimal? StopLoss = null, decimal? TakeProfit = null);
}
