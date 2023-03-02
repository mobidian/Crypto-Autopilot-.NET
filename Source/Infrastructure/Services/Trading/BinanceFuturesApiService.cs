using System.Text;

using Application.Exceptions;
using Application.Interfaces.Services.Trading;
using Application.Interfaces.Services.Trading.Monitors;

using Binance.Net.Enums;
using Binance.Net.Interfaces.Clients.UsdFuturesApi;
using Binance.Net.Objects.Models.Futures;

using Domain.Extensions;

using Infrastructure.Extensions;

using Polly;

namespace Infrastructure.Services.Trading;

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

    public async Task<IEnumerable<BinanceFuturesOrder>> PlaceMarketOrderAsync(string currencyPair, OrderSide orderSide, decimal Margin, decimal Leverage, decimal? StopLoss = null, decimal? TakeProfit = null)
    {
        var currentPrice = await this.MarketDataProvider.GetCurrentPriceAsync(currencyPair);
        ValidateTpSl(orderSide, currentPrice, "current price", StopLoss, TakeProfit);

        
        var batchOrders = CreateBatchOrders(currencyPair, orderSide, currentPrice, Margin, Leverage, StopLoss, TakeProfit);
        var callResult = await this.TradingClient.PlaceMultipleOrdersAsync(batchOrders);
        callResult.ThrowIfHasError();

        var placedOrders = callResult.Data.Select(x => x.Data).ToArray();
        return this.GetOrdersFromPlacedOrders(placedOrders);
    }
    private static BinanceFuturesBatchOrder[] CreateBatchOrders(string currencyPair, OrderSide orderSide, decimal currentPrice, decimal Margin, decimal Leverage, decimal? StopLoss = null, decimal? TakeProfit = null)
    {
        var positionSide = orderSide.ToPositionSide();
        var inverseOrderSide = orderSide.Invert();
        var Quantity = Math.Round(Margin * Leverage / currentPrice, 3);
        
        var BatchOrders = new List<BinanceFuturesBatchOrder>()
        {
            new BinanceFuturesBatchOrder
            {
                Symbol = currencyPair,
                Side = orderSide,
                Type = FuturesOrderType.Market,
                PositionSide = positionSide,
                Quantity = Quantity,
            }
        };

        if (StopLoss.HasValue)
        {
            BatchOrders.Add(new BinanceFuturesBatchOrder
            {
                Symbol = currencyPair,
                Side = inverseOrderSide,
                Type = FuturesOrderType.StopMarket,
                PositionSide = positionSide,
                Quantity = Quantity,
                StopPrice = Math.Round(StopLoss.Value, 2),
                TimeInForce = TimeInForce.GoodTillCanceled
            });
        }
        
        if (TakeProfit.HasValue)
        {
            BatchOrders.Add(new BinanceFuturesBatchOrder
            {
                Symbol = currencyPair,
                Side = inverseOrderSide,
                Type = FuturesOrderType.TakeProfitMarket,
                PositionSide = positionSide,
                Quantity = Quantity,
                StopPrice = Math.Round(TakeProfit.Value, 2),
                TimeInForce = TimeInForce.GoodTillCanceled
            });
        }

        return BatchOrders.ToArray();
    }

    public async Task<BinanceFuturesOrder> PlaceLimitOrderAsync(string currencyPair, OrderSide orderSide, decimal LimitPrice, decimal Margin, decimal Leverage, decimal? StopLoss = null, decimal? TakeProfit = null)
    {
        var currentPrice = await this.MarketDataProvider.GetCurrentPriceAsync(currencyPair);
        var Quantity = Math.Round(Margin * Leverage / currentPrice, 3);
        
        ValidateLimitOrderInput(orderSide, LimitPrice, StopLoss, TakeProfit, currentPrice);


        var callResult = await this.TradingClient.PlaceOrderAsync(currencyPair, orderSide, FuturesOrderType.Limit, Quantity, LimitPrice, orderSide.ToPositionSide(), TimeInForce.GoodTillCanceled);
        callResult.ThrowIfHasError();

        var placedLimitOrder = callResult.Data;
        
        if (StopLoss is not null || TakeProfit is not null)
            _ = OtocoImplementationAsync(placedLimitOrder, StopLoss, TakeProfit, Quantity);

        return await this.GetOrderFromPlacedOrderAndValidateAsync(placedLimitOrder);
    }
    private static void ValidateLimitOrderInput(OrderSide orderSide, decimal LimitPrice, decimal? StopLoss, decimal? TakeProfit, decimal currentPrice)
    {
        if (orderSide == OrderSide.Buy && LimitPrice > currentPrice)
            throw new InvalidOrderException("The limit price for a buy order can't be greater than the current price");

        if (orderSide == OrderSide.Sell && LimitPrice < currentPrice)
            throw new InvalidOrderException("The limit price for a sell order can't be less greater than the current price");
        
        ValidateTpSl(orderSide, LimitPrice, "limit price", StopLoss, TakeProfit);
    }
    private async Task OtocoImplementationAsync(BinanceFuturesPlacedOrder placedLimitOrder, decimal? StopLoss, decimal? TakeProfit, decimal Quantity)
    {
        try
        {
            await this.OrderStatusMonitor.SubscribeToOrderUpdatesAsync();
            await this.OrderStatusMonitor.WaitForOrderStatusAsync(placedLimitOrder.Id, OrderStatus.Filled);
            
            var slTpBatchOrders = CreateTpSlBatchOrders(placedLimitOrder, StopLoss, TakeProfit, Quantity);
            var callResult = await this.TradingClient.PlaceMultipleOrdersAsync(slTpBatchOrders);
            callResult.ThrowIfHasError();
            
            var slTpPlacedOrders = callResult.Data.Select(x => x.Data);
            
            var positionOrders = slTpPlacedOrders.Prepend(placedLimitOrder).ToArray();
        }
        catch (Exception)
        {
            // // TODO Exception notification publishing with mediator // //
            throw;
        }
    }
    private static BinanceFuturesBatchOrder[] CreateTpSlBatchOrders(BinanceFuturesPlacedOrder placedLimitOrder, decimal? StopLoss, decimal? TakeProfit, decimal Quantity)
    {
        var entryOrderSide = placedLimitOrder.Side;
        
        var currencyPair = placedLimitOrder.Symbol;

        var positionSide = entryOrderSide.ToPositionSide();
        var inverseOrderSide = entryOrderSide.Invert();
        
        var BatchOrders = new List<BinanceFuturesBatchOrder>();

        if (StopLoss.HasValue)
        {
            BatchOrders.Add(new BinanceFuturesBatchOrder
            {
                Symbol = currencyPair,
                Side = inverseOrderSide,
                Type = FuturesOrderType.StopMarket,
                PositionSide = positionSide,
                Quantity = Quantity,
                StopPrice = Math.Round(StopLoss.Value, 2),
                TimeInForce = TimeInForce.GoodTillCanceled
            });
        }
        
        if (TakeProfit.HasValue)
        {
            BatchOrders.Add(new BinanceFuturesBatchOrder
            {
                Symbol = currencyPair,
                Side = inverseOrderSide,
                Type = FuturesOrderType.TakeProfitMarket,
                PositionSide = positionSide,
                Quantity = Quantity,
                StopPrice = Math.Round(TakeProfit.Value, 2),
                TimeInForce = TimeInForce.GoodTillCanceled
            });
        }

        return BatchOrders.ToArray();
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
        var futuresOrders = Enumerable.Range(0, 3).Select(_ => new BinanceFuturesOrder()).ToArray();
        
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
