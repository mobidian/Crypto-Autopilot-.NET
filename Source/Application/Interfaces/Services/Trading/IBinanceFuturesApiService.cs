using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;

namespace Application.Interfaces.Services.Trading;

/// <summary>
/// Client for interacting with the Binance USDⓈ-M Futures markets
/// </summary>
public interface IBinanceFuturesApiService
{
    /// <summary>
    /// Places a new market order at the current market price and optionally places a stop loss and a take profit
    /// </summary>
    /// <param name="currencyPair">The name of the cryptocurrency contract to trade</param>
    /// <param name="orderSide">The side of the order</param>
    /// <param name="Margin">The margin for the position</param>
    /// <param name="Leverage">The leverage to use for the position</param>
    /// <param name="StopLoss">The stop loss price</param>
    /// <param name="TakeProfit">The take profit price</param>
    /// <returns>The orders that have been placed</returns>
    public Task<IEnumerable<BinanceFuturesOrder>> PlaceMarketOrderAsync(string currencyPair, OrderSide orderSide, decimal Margin, decimal Leverage, decimal? StopLoss = null, decimal? TakeProfit = null);
    
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

    /// <summary>
    /// Cancels a pending order
    /// </summary>
    /// <param name="currencyPair"></param>
    /// <param name="OrderID"></param>
    /// <returns>The order that has been cancelled</returns>
    public Task<BinanceFuturesCancelOrder> CancelOrderAsync(string currencyPair, long OrderID);
    
    /// <summary>
    /// Cancels multiple pending orders
    /// </summary>
    /// <param name="currencyPair"></param>
    /// <param name="OrderIDs"></param>
    /// <returns>The orders that have been cancelled</returns>
    public Task<IEnumerable<BinanceFuturesCancelOrder>> CancelOrdersAsync(string currencyPair, List<long> OrderIDs);
}
