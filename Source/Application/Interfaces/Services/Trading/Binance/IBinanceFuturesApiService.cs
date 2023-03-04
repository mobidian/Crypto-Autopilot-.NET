using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;

namespace Application.Interfaces.Services.Trading.Binance;

/// <summary>
/// Client for interacting with the Binance USDⓈ-M Futures markets
/// </summary>
public interface IBinanceFuturesApiService
{
    /// <summary>
    /// Places a new order
    /// </summary>
    /// <param name="currencyPair">The name of the cryptocurrency contract the order is for</param>
    /// <param name="side">The order side (buy/sell)</param>
    /// <param name="type">The order type</param>
    /// <param name="timeInForce">Lifetime of the order (GoodTillCancel/ImmediateOrCancel/FillOrKill)</param>
    /// <param name="quantity">The quantity of the base symbol</param>
    /// <param name="positionSide">The position side</param>
    /// <param name="reduceOnly">Specify as true if the order is intended to only reduce the position</param>
    /// <param name="price">The price to use</param>
    /// <param name="newClientOrderId">Unique id for order</param>
    /// <param name="stopPrice">Used for stop orders</param>
    /// <param name="activationPrice">Used with TRAILING_STOP_MARKET orders, default as the latest price（supporting different workingType)</param>
    /// <param name="callbackRate">Used with TRAILING_STOP_MARKET orders</param>
    /// <param name="workingType">stopPrice triggered by: "MARK_PRICE", "CONTRACT_PRICE"</param>
    /// <param name="closePosition">Close-All，used with STOP_MARKET or TAKE_PROFIT_MARKET.</param>
    /// <param name="orderResponseType">The response type. Default Acknowledge</param>
    /// <param name="priceProtect">If true when price reaches stopPrice, difference between "MARK_PRICE" and "CONTRACT_PRICE" cannot be larger than "triggerProtect" of the symbol.</param>
    /// <param name="receiveWindow">The receive window for which this request is active. When the request takes longer than this to complete the server will reject the request</param>
    /// <returns></returns>
    public Task<BinanceFuturesOrder> PlaceOrderAsync(string currencyPair, OrderSide side, FuturesOrderType type, decimal? quantity, decimal? price = null, PositionSide? positionSide = null, TimeInForce? timeInForce = null, bool? reduceOnly = null, string? newClientOrderId = null, decimal? stopPrice = null, decimal? activationPrice = null, decimal? callbackRate = null, WorkingType? workingType = null, bool? closePosition = null, OrderResponseType? orderResponseType = null, bool? priceProtect = null, int? receiveWindow = null);

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
