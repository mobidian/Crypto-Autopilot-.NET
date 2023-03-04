using System.Text;

using Application.Exceptions;
using Application.Interfaces.Services.Trading.Binance;
using Application.Interfaces.Services.Trading.Binance.Monitors;

using Binance.Net.Enums;
using Binance.Net.Interfaces.Clients.UsdFuturesApi;
using Binance.Net.Objects.Models.Futures;

using Domain.Extensions;

using Infrastructure.Extensions;

using Polly;

namespace Infrastructure.Services.Trading.Binance;

public class BinanceFuturesApiService : IBinanceFuturesApiService
{
    private readonly IBinanceClientUsdFuturesApiTrading TradingClient;
    private readonly IFuturesMarketDataProvider MarketDataProvider;
    private readonly IOrderStatusMonitor OrderStatusMonitor;

    public BinanceFuturesApiService(IBinanceClientUsdFuturesApiTrading tradingClient, IFuturesMarketDataProvider marketDataProvider, IOrderStatusMonitor orderStatusMonitor)
    {
        this.TradingClient = tradingClient;
        this.MarketDataProvider = marketDataProvider;
        this.OrderStatusMonitor = orderStatusMonitor;
    }

    //// //// ////

    private readonly IAsyncPolicy<BinanceFuturesOrder> GetOrderRetryPolicy =
        Policy<BinanceFuturesOrder>
            .Handle<Exception>()
            .WaitAndRetryAsync(3, retryCount => TimeSpan.FromSeconds(Math.Round(Math.Pow(1.6, retryCount), 2))); // 1.6 sec, 2.56 sec, 4.1 sec


    public async Task<BinanceFuturesOrder> PlaceOrderAsync(string currencyPair, OrderSide side, FuturesOrderType type, decimal? quantity, decimal? price = null, PositionSide? positionSide = null, TimeInForce? timeInForce = null, bool? reduceOnly = null, string? newClientOrderId = null, decimal? stopPrice = null, decimal? activationPrice = null, decimal? callbackRate = null, WorkingType? workingType = null, bool? closePosition = null, OrderResponseType? orderResponseType = null, bool? priceProtect = null, int? receiveWindow = null)
    {
        var callResult = await this.TradingClient.PlaceOrderAsync(currencyPair, side, type, quantity, price, positionSide, timeInForce, reduceOnly, newClientOrderId, stopPrice, activationPrice, callbackRate, workingType, closePosition, orderResponseType, priceProtect, receiveWindow);
        callResult.ThrowIfHasError();

        return await this.GetOrderFromPlacedOrderAndValidateAsync(callResult.Data);
    }

    public async Task<IEnumerable<BinanceFuturesOrder>> PlaceMarketOrderAsync(string currencyPair, OrderSide orderSide, decimal Margin, decimal Leverage, decimal? StopLoss = null, decimal? TakeProfit = null)
    {
        var currentPrice = await this.MarketDataProvider.GetCurrentPriceAsync(currencyPair);
        ValidateTpSl(orderSide, currentPrice, "current price", StopLoss, TakeProfit);


        var Quantity = Math.Round(Margin * Leverage / currentPrice, 3);
        var callResult = await this.TradingClient.PlaceOrderAsync(currencyPair, orderSide, FuturesOrderType.Market, Quantity, positionSide: orderSide.ToPositionSide());
        callResult.ThrowIfHasError();

        var placedStopLossTakeProfitOrders = await this.PlaceStopLossTakeProfitAsync(currencyPair, orderSide, StopLoss, TakeProfit);

        return this.GetOrdersFromPlacedOrders(placedStopLossTakeProfitOrders.Prepend(callResult.Data).ToArray());
    }
    private async Task<IEnumerable<BinanceFuturesPlacedOrder>> PlaceStopLossTakeProfitAsync(string currencyPair, OrderSide orderSide, decimal? StopLoss = null, decimal? TakeProfit = null)
    {
        var positionSide = orderSide.ToPositionSide();
        var inverseOrderSide = orderSide.Invert();

        var orders = new BinanceFuturesPlacedOrder?[] { null, null };
        var placeStopLossAsync = async () =>
        {
            if (StopLoss is not null)
            {
                var callResult = await this.TradingClient.PlaceOrderAsync(currencyPair, inverseOrderSide, FuturesOrderType.StopMarket, 0, positionSide: positionSide, stopPrice: Math.Round(StopLoss.Value, 2), timeInForce: TimeInForce.GoodTillCanceled, closePosition: true);
                callResult.ThrowIfHasError();
                orders[0] = callResult.Data;
            }
        };
        var placeTakeProfitAsync = async () =>
        {
            if (TakeProfit is not null)
            {
                var callResult = await this.TradingClient.PlaceOrderAsync(currencyPair, inverseOrderSide, FuturesOrderType.TakeProfitMarket, 0, positionSide: positionSide, stopPrice: Math.Round(TakeProfit.Value, 2), timeInForce: TimeInForce.GoodTillCanceled, closePosition: true);
                callResult.ThrowIfHasError();
                orders[1] = callResult.Data;
            }
        };

        await Task.WhenAll(placeStopLossAsync.Invoke(), placeTakeProfitAsync.Invoke());

        return orders.Where(x => x is not null)!;
    }


    public async Task<BinanceFuturesCancelOrder> CancelOrderAsync(string currencyPair, long OrderID)
    {
        var callResult = await this.TradingClient.CancelOrderAsync(currencyPair, OrderID);
        callResult.ThrowIfHasError();

        return callResult.Data;
    }

    public async Task<IEnumerable<BinanceFuturesCancelOrder>> CancelOrdersAsync(string currencyPair, List<long> OrderIDs)
    {
        var callResult = await this.TradingClient.CancelMultipleOrdersAsync(currencyPair, OrderIDs);
        callResult.ThrowIfHasError();

        return callResult.Data.Select(x => x.Data);
    }


    private static void ValidateTpSl(OrderSide orderSide, decimal price, string priceType, decimal? stopLoss, decimal? takeProfit)
    {
        var builder = new StringBuilder();

        #region SL/TP validate positive
        if (stopLoss <= 0)
            builder.AppendLine($"Specified {nameof(stopLoss)} was negative");

        if (takeProfit <= 0)
            builder.AppendLine($"Specified {nameof(takeProfit)} was negative");

        if (builder.Length != 0)
            throw new InvalidOrderException(builder.Remove(builder.Length - 1, 1).ToString());
        #endregion

        #region SL/TP validate against the price
        if (orderSide == OrderSide.Buy)
        {
            if (stopLoss >= price)
                builder.AppendLine($"The stop loss can't be greater than or equal to the {priceType} for a {orderSide.ToString().ToLower()} order, {priceType} was {price} and stop loss was {stopLoss}");

            if (takeProfit <= price)
                builder.AppendLine($"The take profit can't be less greater than or equal to the {priceType} for a {orderSide.ToString().ToLower()} order, {priceType} was {price} and take profit was {takeProfit}");
        }
        else
        {
            if (stopLoss <= price)
                builder.AppendLine($"The stop loss can't be less greater than or equal to the {priceType} for a {orderSide.ToString().ToLower()} order, {priceType} was {price} and stop loss was {stopLoss}");

            if (takeProfit >= price)
                builder.AppendLine($"The take profit can't be greater than or equal to the {priceType} for a {orderSide.ToString().ToLower()} order, {priceType} was {price} and take profit was {takeProfit}");
        }

        if (builder.Length != 0)
            throw new InvalidOrderException(builder.Remove(builder.Length - 1, 1).ToString());
        #endregion
    }

    private BinanceFuturesOrder[] GetOrdersFromPlacedOrders(BinanceFuturesPlacedOrder[] placedOrders)
    {
        var futuresOrders = Enumerable.Range(0, placedOrders.Length).Select(_ => new BinanceFuturesOrder()).ToArray();

        Parallel.For(0, placedOrders.Length, i =>
        {
            var task = this.GetOrderFromPlacedOrderAndValidateAsync(placedOrders[i]);
            futuresOrders[i] = task.GetAwaiter().GetResult();
        });

        //await Parallel.ForEachAsync(placedOrders, async (placedOrder, _) =>
        //{
        //    var index = Array.IndexOf(placedOrders, placedOrder);
        //    futuresOrders[index] = await this.GetOrderFromPlacedOrderAndValidateAsync(placedOrder);
        //});

        return futuresOrders;
    }
    private async Task<BinanceFuturesOrder> GetOrderFromPlacedOrderAndValidateAsync(BinanceFuturesPlacedOrder placedOrder)
        => await this.GetOrderRetryPolicy.ExecuteAsync(async () =>
        {
            var futuresOrder = await this.MarketDataProvider.GetOrderAsync(placedOrder.Symbol, placedOrder.Id);

            if (futuresOrder is null)
                return new BinanceFuturesOrder();

            if (futuresOrder.Symbol != placedOrder.Symbol ||
                futuresOrder.Id != placedOrder.Id ||
                futuresOrder.ClientOrderId != placedOrder.ClientOrderId ||
                futuresOrder.Side != placedOrder.Side ||
                futuresOrder.Type != placedOrder.Type ||
                futuresOrder.WorkingType != placedOrder.WorkingType ||
                futuresOrder.PositionSide != placedOrder.PositionSide)
                throw new InternalTradingServiceException();

            return futuresOrder;
        });
}
