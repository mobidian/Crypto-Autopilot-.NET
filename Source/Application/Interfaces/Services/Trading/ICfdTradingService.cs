using Application.Exceptions;

using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;

using Domain.Models;

namespace Application.Interfaces.Services.Trading;

public interface ICfdTradingService : IDisposable
{
    public CurrencyPair CurrencyPair { get; }
    public decimal Leverage { get; }
    public FuturesPosition? Position { get; }

    /////  /////  /////

    /// <summary>
    /// Places a new order at the currect market price
    /// </summary>
    /// <param name="OrderSide">The side of the order to be placed</param>
    /// <param name="QuoteMargin">The margin for the order to be placed</param>
    /// <param name="StopLoss">The stop loss for the order to be placed</param>
    /// <param name="TakeProfit">The take profit for the order to be placed</param>
    /// <returns>The orders that have been placed</returns>
    /// <exception cref="InvalidOrderException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="InternalTradingServiceException"></exception>
    public Task<IEnumerable<BinanceFuturesPlacedOrder>> PlaceMarketOrderAsync(OrderSide OrderSide, decimal QuoteMargin = decimal.MaxValue, decimal? StopLoss = null, decimal? TakeProfit = null);
    
    /// <summary>
    /// Places a new limit order
    /// </summary>
    /// <param name="OrderSide">The side of the order to be placed</param>
    /// <param name="LimitPrice">The limit price of the order to be placed</param>
    /// <param name="QuoteMargin">The margin for the order to be placed</param>
    /// <param name="StopLoss">The stop loss for the order to be placed</param>
    /// <param name="TakeProfit">The take profit for the order to be placed</param>
    /// <returns>The order that has been placed</returns>
    /// <exception cref="InvalidOrderException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="InternalTradingServiceException"></exception>
    public Task<BinanceFuturesPlacedOrder> PlaceLimitOrderAsync(OrderSide OrderSide, decimal LimitPrice, decimal QuoteMargin = decimal.MaxValue, decimal? StopLoss = null, decimal? TakeProfit = null);

    /// <summary>
    /// Places a stop loss limit order if there is an open position and then updates this.Position.StopLossOrder
    /// </summary>
    /// <param name="price">The price for the stop loss order to be placed</param>
    /// <returns>The order that has been placed as a <see cref="BinanceFuturesPlacedOrder"/></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="InternalTradingServiceException"></exception>
    public Task<BinanceFuturesPlacedOrder> PlaceStopLossAsync(decimal price);
    
    /// <summary>
    /// Places a take profit limit order if there is an open position and then updates this.Position.TakeProfitOrder
    /// </summary>
    /// <param name="price">The price for the stop loss order to be placed</param>
    /// <returns>The order that has been placed as a <see cref="BinanceFuturesPlacedOrder"/></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="InternalTradingServiceException"></exception>
    public Task<BinanceFuturesPlacedOrder> PlaceTakeProfitAsync(decimal price);

    /// <summary>
    /// Closes the existing position
    /// </summary>
    /// <returns>The futures order that was placed to close the position</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InternalTradingServiceException"></exception>
    public Task<BinanceFuturesOrder> ClosePositionAsync();


    public bool IsInPosition();
}
