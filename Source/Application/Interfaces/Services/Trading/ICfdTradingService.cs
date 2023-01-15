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

    public Task<BinanceFuturesOrder> GetOrderAsync(long orderID);
    public Task<BinanceFuturesOrder> GetOrderAsync(BinanceFuturesOrder order) => GetOrderAsync(order.Id);
    public Task<BinanceFuturesOrder> GetOrderAsync(BinanceFuturesPlacedOrder placedOrder) => GetOrderAsync(placedOrder.Id);

    /////  /////

    public Task<decimal> GetCurrentPriceAsync();
    public Task<decimal> GetEquityAsync();

    public Task<IEnumerable<BinanceFuturesPlacedOrder>> OpenPositionAtMarketPriceAsync(OrderSide OrderSide, decimal MarginBUSD = decimal.MaxValue, decimal? StopLoss_price = null, decimal? TakeProfit_price = null);
    public Task<BinanceFuturesOrder> ClosePositionAsync();
    public Task<BinanceFuturesPlacedOrder> PlaceStopLossAsync(decimal price);


    public bool IsInPosition();
}
