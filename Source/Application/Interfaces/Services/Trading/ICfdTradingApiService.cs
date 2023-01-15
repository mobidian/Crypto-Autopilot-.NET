using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;

using CryptoExchange.Net.Objects;

using Domain.Models;

namespace Application.Interfaces.Services.Trading;

public interface ICfdTradingService : IDisposable
{
    public CurrencyPair CurrencyPair { get; }
    public decimal Leverage { get; }
    public FuturesPosition? Position { get; }

    /////  /////  /////

    public Task<CallResult<BinanceFuturesOrder>> GetOrderAsync(long orderID);
    public Task<CallResult<BinanceFuturesOrder>> GetOrderAsync(BinanceFuturesOrder order) => GetOrderAsync(order.Id);
    public Task<CallResult<BinanceFuturesOrder>> GetOrderAsync(BinanceFuturesPlacedOrder placedOrder) => GetOrderAsync(placedOrder.Id);

    /////  /////

    public Task<decimal> GetCurrentPriceAsync();
    public Task<decimal> GetEquityAsync();

    public Task<CallResult<IEnumerable<CallResult<BinanceFuturesPlacedOrder>>>> OpenPositionAtMarketPriceAsync(OrderSide OrderSide, decimal MarginBUSD = decimal.MaxValue, decimal? StopLoss_price = null, decimal? TakeProfit_price = null);
    public Task<CallResult<BinanceFuturesOrder>> ClosePositionAsync();
    public Task<CallResult<BinanceFuturesPlacedOrder>> PlaceStopLossAsync(decimal price);


    public bool IsInPosition() => Position is not null;
}
