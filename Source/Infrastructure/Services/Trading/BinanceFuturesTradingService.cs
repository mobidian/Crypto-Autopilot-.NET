using System.Text;

using Application.Exceptions;
using Application.Interfaces.Services.Trading;
using Application.Interfaces.Services.Trading.Monitors;

using Binance.Net.Enums;
using Binance.Net.Interfaces.Clients.UsdFuturesApi;
using Binance.Net.Objects.Models.Futures;

using CryptoExchange.Net.Objects;

using Domain.Extensions;
using Domain.Models;

using Infrastructure.Extensions;

using Polly;

namespace Infrastructure.Services.Trading;

public class BinanceFuturesTradingService : IFuturesTradingService
{
    public CurrencyPair CurrencyPair { get; }
    public decimal Leverage { get; }

    private readonly IBinanceClientUsdFuturesApiTrading TradingClient;
    private readonly IBinanceFuturesAccountDataProvider AccountDataProvider;
    private readonly IFuturesMarketDataProvider MarketDataProvider;
    private readonly IOrderStatusMonitor OrderStatusMonitor;
    
    public FuturesPosition? Position { get; private set; }

    private readonly int NrDecimals = 2;
    
    public BinanceFuturesTradingService(CurrencyPair currencyPair, decimal leverage, IBinanceClientUsdFuturesApiTrading tradingClient, IBinanceFuturesAccountDataProvider accountDataProvider, IFuturesMarketDataProvider marketDataProvider, IOrderStatusMonitor orderStatusMonitor)
    {
        this.CurrencyPair = currencyPair ?? throw new ArgumentNullException(nameof(currencyPair));
        this.Leverage = leverage;
        
        this.TradingClient = tradingClient ?? throw new ArgumentNullException(nameof(tradingClient));
        this.AccountDataProvider = accountDataProvider ?? throw new ArgumentNullException(nameof(accountDataProvider));
        this.MarketDataProvider = marketDataProvider ?? throw new ArgumentNullException(nameof(marketDataProvider));
        this.OrderStatusMonitor = orderStatusMonitor ?? throw new ArgumentNullException(nameof(orderStatusMonitor));
    }

    //// //// ////

    #region Retry Policies
    private readonly IAsyncPolicy<BinanceFuturesOrder> MarketOrderRetryPolicy =
        Policy<BinanceFuturesOrder>
            .Handle<Exception>()
            .WaitAndRetryAsync(3, retryCount => TimeSpan.FromSeconds(Math.Round(Math.Pow(1.6, retryCount), 2))); // 1.6 sec, 2.56 sec, 4.1 sec
    
    private readonly IAsyncPolicy<WebCallResult<BinanceFuturesPlacedOrder>> SlTpRetryPolicy =
        Policy<WebCallResult<BinanceFuturesPlacedOrder>>
            .Handle<InternalTradingServiceException>(exc =>
            {
                // These if statements check for specific error messages related to user input errors
                // Errors raised as a result of bad user input won't be handled
                
                if (exc.Message.EndsWith("Error: -1102: Mandatory parameter 'stopPrice' was not sent, was empty/null, or malformed."))
                    return false;

                if (exc.Message.EndsWith("Error: -2021: Order would immediately trigger."))
                    return false;
                
                return true;
            })
            .WaitAndRetryAsync(3, retryCount => TimeSpan.FromSeconds(Math.Round(Math.Pow(1.6, retryCount), 2))); // 1.6 sec, 2.56 sec, 4.1 sec
    #endregion

    public async Task<IEnumerable<BinanceFuturesPlacedOrder>> PlaceMarketOrderAsync(OrderSide OrderSide, decimal QuoteMargin = decimal.MaxValue, decimal? StopLoss = null, decimal? TakeProfit = null)
    {
        if (this.IsInPosition())
        {
            throw new InvalidOperationException("A position is open already");
        }

        (var BaseQuantity, var CurrentPrice) = await this.Get_BaseQuantity_and_CurrentPrice_Async(QuoteMargin);
        ValidateTpSl(OrderSide, CurrentPrice, StopLoss, TakeProfit);


        var BatchOrders = this.CreateBatchOrdersForNewMarketOrder(OrderSide, StopLoss, TakeProfit, BaseQuantity);
        var PlacedOrdersCallResult = await this.TradingClient.PlaceMultipleOrdersAsync(BatchOrders);
        PlacedOrdersCallResult.ThrowIfHasError();
        
        var PlacedOrders = PlacedOrdersCallResult.Data.Select(call => call.Data).Where(placedOrder => placedOrder is not null).ToArray();
        this.Position = await this.CreateFuturesPositionInstanceAsync(QuoteMargin, PlacedOrders);
        
        return PlacedOrdersCallResult.Data.Select(callRes => callRes.Data);
    }
    private async Task<(decimal BaseQuantity, decimal CurrentPrice)> Get_BaseQuantity_and_CurrentPrice_Async(decimal QuoteMargin)
    {
        decimal BaseQuantity;
        decimal equityBUSD;
        decimal CurrentPrice;
        
        var GetCurrentPrice_Task = this.MarketDataProvider.GetCurrentPriceAsync(this.CurrencyPair.Name);
        equityBUSD = QuoteMargin == decimal.MaxValue ? await this.AccountDataProvider.GetEquityAsync(this.CurrencyPair.Name) : QuoteMargin;
        CurrentPrice = await GetCurrentPrice_Task;

        BaseQuantity = Math.Round(equityBUSD * this.Leverage / CurrentPrice, this.NrDecimals, MidpointRounding.ToZero);

        return (BaseQuantity, CurrentPrice);
    }
    private static void ValidateTpSl(OrderSide OrderSide, decimal Price, decimal? StopLoss, decimal? TakeProfit_price)
    {
        var builder = new StringBuilder();

        #region SL/TP validate positive
        if (StopLoss <= 0)
            builder.AppendLine($"Specified {nameof(StopLoss)} was negative");

        if (TakeProfit_price <= 0)
            builder.AppendLine($"Specified {nameof(TakeProfit_price)} was negative");

        if (builder.Length != 0)
            throw new InvalidOrderException(builder.Remove(builder.Length - 1, 1).ToString());
        #endregion

        #region SL/TP validate against current price
        if (OrderSide == OrderSide.Buy)
        {
            if (StopLoss >= Price)
                builder.AppendLine($"The stop loss can't be greater than or equal to the current price for a {OrderSide} order, current price was {Price} and stop loss was {StopLoss}");

            if (TakeProfit_price <= Price)
                builder.AppendLine($"The take profit can't be less greater than or equal to the current price for a {OrderSide} order, current price was {Price} and take profit was {TakeProfit_price}");
        }
        else
        {
            if (StopLoss <= Price)
                builder.AppendLine($"The stop loss can't be less greater than or equal to the current price for a {OrderSide} order, current price was {Price} and stop loss was {StopLoss}");

            if (TakeProfit_price >= Price)
                builder.AppendLine($"The take profit can't be greater than or equal to the current price for a {OrderSide} order, current price was {Price} and take profit was {TakeProfit_price}");
        }

        if (builder.Length != 0)
            throw new InvalidOrderException(builder.Remove(builder.Length - 1, 1).ToString());
        #endregion
    }
    private BinanceFuturesBatchOrder[] CreateBatchOrdersForNewMarketOrder(OrderSide OrderSide, decimal? StopLoss, decimal? TakeProfit, decimal BaseQuantity)
    {
        var symbolName = this.CurrencyPair.Name;
        var positionSide = OrderSide.ToPositionSide();
        var inverseOrderSide = OrderSide.Invert();

        var BatchOrders = new List<BinanceFuturesBatchOrder>()
        {
            new BinanceFuturesBatchOrder
            {
                Symbol = symbolName,
                Side = OrderSide,
                Type = FuturesOrderType.Market,
                PositionSide = positionSide,
                Quantity = BaseQuantity,
            }
        };
        if (StopLoss.HasValue)
        {
            BatchOrders.Add(new BinanceFuturesBatchOrder
            {
                Symbol = symbolName,
                Side = inverseOrderSide,
                Type = FuturesOrderType.StopMarket,
                PositionSide = positionSide,
                Quantity = BaseQuantity,
                StopPrice = Math.Round(StopLoss.Value, this.NrDecimals),
            });
        }
        if (TakeProfit.HasValue)
        {
            BatchOrders.Add(new BinanceFuturesBatchOrder
            {
                Symbol = symbolName,
                Side = inverseOrderSide,
                Type = FuturesOrderType.TakeProfitMarket,
                PositionSide = positionSide,
                Quantity = BaseQuantity,
                StopPrice = Math.Round(TakeProfit.Value, this.NrDecimals),
            });
        }
           
        return BatchOrders.ToArray();
    }
    private async Task<FuturesPosition> CreateFuturesPositionInstanceAsync(decimal QuoteMargin, BinanceFuturesPlacedOrder[] PlacedOrders)
    {
        try
        {
            var FuturesOrders = Enumerable.Range(0, 3).Select(_ => new BinanceFuturesOrder()).ToArray();
            
            Parallel.For(0, PlacedOrders.Length, i =>
            {
                var task = this.GetOrderFromPlacedOrderAndValidateAsync(PlacedOrders[i]);
                FuturesOrders[i] = task.GetAwaiter().GetResult();
            });

            // await Parallel.ForEachAsync(PlacedOrders, async (placedOrder, _) =>
            // {
            //     var index = Array.IndexOf(PlacedOrders, placedOrder);
            //     FuturesOrders[index] = await this.GetOrderFromPlacedOrderAndValidateAsync(placedOrder);
            // });

            return new FuturesPosition
            {
                CurrencyPair = this.CurrencyPair,

                Leverage = this.Leverage,
                Margin = QuoteMargin,

                EntryOrder = FuturesOrders[0],
                StopLossOrder = FuturesOrders[1].Id != 0 ? FuturesOrders[1] : null,
                TakeProfitOrder = FuturesOrders[2].Id != 0 ? FuturesOrders[2] : null
            };
        }
        catch
        {
            var entryOrder = PlacedOrders.First();
            var task1 = this.TradingClient.PlaceOrderAsync(this.CurrencyPair.Name, entryOrder.Side.Invert(), FuturesOrderType.Market, entryOrder.Quantity, positionSide: entryOrder.PositionSide);
            var task2 = this.TradingClient.CancelMultipleOrdersAsync(this.CurrencyPair.Name, PlacedOrders.Select(x => x.Id).ToList());
            
            await Task.WhenAll(task1, task2);

            throw;
        }
    }
    public async Task<BinanceFuturesOrder> GetOrderFromPlacedOrderAndValidateAsync(BinanceFuturesPlacedOrder placedOrder)
        => await this.MarketOrderRetryPolicy.ExecuteAsync(async () =>
        {
            var futuresOrder = await this.MarketDataProvider.GetOrderAsync(this.CurrencyPair.Name, placedOrder.Id);

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

    public async Task<BinanceFuturesPlacedOrder> PlaceLimitOrderAsync(OrderSide OrderSide, decimal LimitPrice, decimal QuoteMargin = decimal.MaxValue, decimal? StopLoss = null, decimal? TakeProfit = null)
    {
        if (this.IsInPosition())
        {
            throw new InvalidOperationException("A position is open already");
        }
        
        (var BaseQuantity, var CurrentPrice) = await this.Get_BaseQuantity_and_CurrentPrice_Async(QuoteMargin);
        
        if (OrderSide == OrderSide.Buy && LimitPrice > CurrentPrice)
            throw new InvalidOrderException("The limit price for a buy order can't be greater than the current price");

        if (OrderSide == OrderSide.Sell && LimitPrice < CurrentPrice)
            throw new InvalidOrderException("The limit price for a sell order can't be less greater than the current price");

        ValidateTpSl(OrderSide, LimitPrice, StopLoss, TakeProfit);

        
        var PlacedOrderCallResult = await this.TradingClient.PlaceOrderAsync(this.CurrencyPair.Name, OrderSide, FuturesOrderType.Limit, BaseQuantity, LimitPrice, OrderSide.ToPositionSide(), TimeInForce.GoodTillCanceled);
        PlacedOrderCallResult.ThrowIfHasError();
        var PlacedLimitOrder = PlacedOrderCallResult.Data;
        
        _ = PlaceTpSlAndUpdatePositionWhenLimitOrderIsFilledAsync(OrderSide, PlacedLimitOrder, QuoteMargin, StopLoss, TakeProfit, BaseQuantity);
        
        return PlacedLimitOrder;
    }
    private async Task PlaceTpSlAndUpdatePositionWhenLimitOrderIsFilledAsync(OrderSide EntryOrderSide, BinanceFuturesPlacedOrder PlacedLimitOrder, decimal QuoteMargin, decimal? StopLoss, decimal? TakeProfit, decimal BaseQuantity)
    {
        await this.OrderStatusMonitor.SubscribeToOrderUpdatesAsync();
        await this.OrderStatusMonitor.WaitForOrderStatusAsync(PlacedLimitOrder.Id, OrderStatus.Filled);
        
        var TpSlPlacedOrdersCallResult = await this.TradingClient.PlaceMultipleOrdersAsync(this.CreateTpSlBatchOrders(EntryOrderSide, StopLoss, TakeProfit, BaseQuantity));
        TpSlPlacedOrdersCallResult.ThrowIfHasError();
        var TpSlPlacedOrders = TpSlPlacedOrdersCallResult.Data.Select(x => x.Data);
        
        this.Position = await this.CreateFuturesPositionInstanceAsync(QuoteMargin, TpSlPlacedOrders.Prepend(PlacedLimitOrder).ToArray());
    }
    private BinanceFuturesBatchOrder[] CreateTpSlBatchOrders(OrderSide EntryOrderSide, decimal? StopLoss, decimal? TakeProfit, decimal BaseQuantity)
    {
        var symbolName = this.CurrencyPair.Name;
        var positionSide = EntryOrderSide.ToPositionSide();
        var inverseOrderSide = EntryOrderSide.Invert();
        
        var BatchOrders = new List<BinanceFuturesBatchOrder>();
        if (StopLoss.HasValue)
        {
            BatchOrders.Add(new BinanceFuturesBatchOrder
            {
                Symbol = symbolName,
                Side = inverseOrderSide,
                Type = FuturesOrderType.StopMarket,
                PositionSide = positionSide,
                Quantity = BaseQuantity,
                StopPrice = Math.Round(StopLoss.Value, this.NrDecimals),
                TimeInForce = TimeInForce.GoodTillCanceled
            });
        }
        if (TakeProfit.HasValue)
        {
            BatchOrders.Add(new BinanceFuturesBatchOrder
            {
                Symbol = symbolName,
                Side = inverseOrderSide,
                Type = FuturesOrderType.TakeProfitMarket,
                PositionSide = positionSide,
                Quantity = BaseQuantity,
                StopPrice = Math.Round(TakeProfit.Value, this.NrDecimals),
                TimeInForce = TimeInForce.GoodTillCanceled
            });
        }
        
        return BatchOrders.ToArray();
    }

    public async Task<BinanceFuturesPlacedOrder> PlaceStopLossAsync(decimal price)
    {
        if (this.Position is null)
        {
            throw new InvalidOperationException("No position is open thus a stop loss can't be placed");
        }


        var SLPlacingCallResult = await this.SlTpRetryPolicy.ExecuteAsync(async () =>
        {
            var SLPlacingCallResult = await this.TradingClient.PlaceOrderAsync(symbol: this.CurrencyPair.Name, side: this.Position.EntryOrder.Side.Invert(), type: FuturesOrderType.StopMarket, quantity: this.Position.EntryOrder.Quantity, stopPrice: Math.Round(price, this.NrDecimals), positionSide: this.Position.Side);
            SLPlacingCallResult.ThrowIfHasError("The stop loss could not be placed");
            return SLPlacingCallResult;
        });
        
        var GetSLOrderTask = this.MarketDataProvider.GetOrderAsync(this.CurrencyPair.Name, SLPlacingCallResult!.Data.Id);
        if (this.Position.StopLossOrder is not null)
        {
            await this.TradingClient.CancelOrderAsync(symbol: this.CurrencyPair.Name, this.Position.StopLossOrder.Id);
        }

        this.Position.StopLossOrder = await GetSLOrderTask;

        return SLPlacingCallResult.Data;
    }

    public async Task<BinanceFuturesPlacedOrder> PlaceTakeProfitAsync(decimal price)
    {
        if (this.Position is null)
        {
            throw new InvalidOperationException("No position is open thus a take profit can't be placed");
        }

        
        var TPPlacingCallResult = await this.SlTpRetryPolicy.ExecuteAsync(async () =>
        {
            var TPPlacingCallResult = await this.TradingClient.PlaceOrderAsync(symbol: this.CurrencyPair.Name, side: this.Position.EntryOrder.Side.Invert(), type: FuturesOrderType.TakeProfitMarket, quantity: this.Position.EntryOrder.Quantity, stopPrice: Math.Round(price, this.NrDecimals), positionSide: this.Position.Side);
            TPPlacingCallResult.ThrowIfHasError("The take profit could not be placed");
            return TPPlacingCallResult;
        });
        
        var GetTPOrderTask = this.MarketDataProvider.GetOrderAsync(this.CurrencyPair.Name, TPPlacingCallResult!.Data.Id);
        if (this.Position.TakeProfitOrder is not null)
        {
            await this.TradingClient.CancelOrderAsync(symbol: this.CurrencyPair.Name, this.Position.TakeProfitOrder.Id);
        }

        this.Position.TakeProfitOrder = await GetTPOrderTask;

        return TPPlacingCallResult.Data;
    }

    public async Task<BinanceFuturesOrder> ClosePositionAsync()
    {
        if (this.Position is null)
        {
            throw new InvalidOperationException($"No position is open", new NullReferenceException($"{nameof(this.Position)} is NULL"));
        }


        var ClosingCallResult = await this.TradingClient.PlaceOrderAsync(symbol: this.CurrencyPair.Name, side: this.Position.EntryOrder.Side.Invert(), type: FuturesOrderType.Market, quantity: this.Position.EntryOrder.Quantity, positionSide: this.Position.Side);
        ClosingCallResult.ThrowIfHasError("The current position could not be closed");
        
        var GetFuturesClosingOrderTask = this.MarketDataProvider.GetOrderAsync(this.CurrencyPair.Name, ClosingCallResult.Data.Id);

        // some orders may still be open so they must be closed
        var CancellingCallResult = await this.TradingClient.CancelMultipleOrdersAsync(symbol: this.CurrencyPair.Name, this.Position.GetOpenOrdersIDs().ToList());
        CancellingCallResult.ThrowIfHasError("The remaining orders could not be closed");

        this.Position = null;
        return await GetFuturesClosingOrderTask;
    }

    public bool IsInPosition() => this.Position is not null;


    //// //// ////


    private bool Disposed = false;

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (this.Disposed)
            return;

        if (disposing)
            this.MarketDataProvider.Dispose();

        this.Disposed = true;
    }
}
