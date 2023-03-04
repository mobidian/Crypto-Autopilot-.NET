using System.Text;

using Application.Exceptions;
using Application.Interfaces.Services.Trading;
using Application.Interfaces.Services.Trading.Monitors;

using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;

using Domain.Extensions;
using Domain.Models;

using Infrastructure.Services.Trading.Internal.Enums;
using Infrastructure.Services.Trading.Internal.Types;

namespace Infrastructure.Services.Trading;

public class BinanceFuturesTradingService : IFuturesTradingService
{
    public CurrencyPair CurrencyPair { get; }
    public decimal Leverage { get; }

    private readonly IBinanceFuturesApiService FuturesApiService;
    private readonly IBinanceFuturesAccountDataProvider AccountDataProvider;
    private readonly IFuturesMarketDataProvider MarketDataProvider;
    private readonly IOrderStatusMonitor OrderStatusMonitor;
    
    public FuturesPosition? Position { get; private set; }
    public BinanceFuturesOrder? LimitOrder { get; private set; }
    
    public BinanceFuturesTradingService(CurrencyPair currencyPair, decimal leverage, IBinanceFuturesApiService futuresApiService, IBinanceFuturesAccountDataProvider accountDataProvider, IFuturesMarketDataProvider marketDataProvider, IOrderStatusMonitor orderStatusMonitor)
    {
        this.CurrencyPair = currencyPair ?? throw new ArgumentNullException(nameof(currencyPair));
        this.Leverage = leverage;
        
        this.FuturesApiService = futuresApiService ?? throw new ArgumentNullException(nameof(futuresApiService));
        this.AccountDataProvider = accountDataProvider ?? throw new ArgumentNullException(nameof(accountDataProvider));
        this.MarketDataProvider = marketDataProvider ?? throw new ArgumentNullException(nameof(marketDataProvider));
        this.OrderStatusMonitor = orderStatusMonitor ?? throw new ArgumentNullException(nameof(orderStatusMonitor));
    }

    //// //// ////
    
    private CancellationTokenSource OcoTaskCts = default!;
    internal StopLossTakeProfitIdPair? OcoIDs = null;
    internal OrderMonitoringTaskStatus OcoTaskStatus = OrderMonitoringTaskStatus.Unstarted;
    
    private CancellationTokenSource OtoTaskCts = default!;
    internal OrderMonitoringTaskStatus OtoTaskStatus = OrderMonitoringTaskStatus.Unstarted;
    
    private CancellationTokenSource OtocoTaskCts = default!;
    internal OrderMonitoringTaskStatus OtocoTaskStatus = OrderMonitoringTaskStatus.Unstarted;

    public async Task<IEnumerable<BinanceFuturesOrder>> PlaceMarketOrderAsync(OrderSide OrderSide, decimal QuoteMargin = decimal.MaxValue, decimal? StopLoss = null, decimal? TakeProfit = null)
    {
        if (this.IsInPosition())
        {
            throw new InvalidOrderException("A position is open already");
        }


        var orders = await this.FuturesApiService.PlaceMarketOrderAsync(this.CurrencyPair.Name, OrderSide, QuoteMargin, this.Leverage, StopLoss, TakeProfit);
        var ordersArray = orders.ToArray();
        
        this.Position = new FuturesPosition
        {
            CurrencyPair = this.CurrencyPair,

            Leverage = this.Leverage,
            Margin = QuoteMargin,

            EntryOrder = ordersArray[0],
            StopLossOrder = ordersArray.Length > 1 ? (ordersArray[1].Id != 0 ? ordersArray[1] : null) : null,
            TakeProfitOrder = ordersArray.Length > 2 ? (ordersArray[2].Id != 0 ? ordersArray[2] : null) : null
        };

        if (this.Position!.StopLossOrder is not null && this.Position.TakeProfitOrder is not null)
            this.InitCtsAndIDsThenFireAndForget_OcoTask();

        return ordersArray;
    }
    private void InitCtsAndIDsThenFireAndForget_OcoTask()
    {
        this.OcoTaskCts = new CancellationTokenSource();
        this.OcoIDs = new StopLossTakeProfitIdPair();
        this.OcoTaskStatus = OrderMonitoringTaskStatus.Unstarted;
        
        this.OcoIDs.StopLoss = this.Position!.StopLossOrder!.Id;
        this.OcoIDs.TakeProfit = this.Position!.TakeProfitOrder!.Id;

        _ = this.OcoTaskAsync();
    }

    public async Task<BinanceFuturesOrder> PlaceLimitOrderAsync(OrderSide OrderSide, decimal LimitPrice, decimal QuoteMargin = decimal.MaxValue, decimal? StopLoss = null, decimal? TakeProfit = null)
    {
        if (this.IsInPosition())
        {
            throw new InvalidOrderException("A position is open already");
        }

        if (this.LimitOrder is not null)
            throw new InvalidOrderException("There is a limit order already");


        var currentPrice = await this.MarketDataProvider.GetCurrentPriceAsync(this.CurrencyPair.Name);
        ValidateLimitOrderInput(OrderSide, LimitPrice, currentPrice, StopLoss, TakeProfit);


        var Quantity = Math.Round(QuoteMargin * this.Leverage / currentPrice, 3);
        var limitOrder = await this.FuturesApiService.PlaceOrderAsync(this.CurrencyPair.Name, OrderSide, FuturesOrderType.Limit, Quantity, LimitPrice, OrderSide.ToPositionSide(), TimeInForce.GoodTillCanceled);
        
        if (StopLoss is not null && TakeProfit is not null)
            this.InitCtsThenFireAndForget_OtocoTask(limitOrder, StopLoss.Value, TakeProfit.Value, QuoteMargin);

        var orderType = StopLoss is not null ? FuturesOrderType.StopMarket : FuturesOrderType.TakeProfitMarket;
        this.InitCtsThenFireAndForget_OtoTask(LimitPrice, QuoteMargin, limitOrder, orderType);
        
        this.LimitOrder = limitOrder;
        return limitOrder;
    }
    private void InitCtsThenFireAndForget_OtocoTask(BinanceFuturesOrder limitOrder, decimal StopLoss, decimal TakeProfit, decimal Margin)
    {
        this.OtocoTaskCts = new CancellationTokenSource();
        this.OtocoTaskStatus = OrderMonitoringTaskStatus.Unstarted;
        
        _ = this.OtocoTaskAsync(limitOrder, StopLoss, TakeProfit, Margin);
    }
    private void InitCtsThenFireAndForget_OtoTask(decimal LimitPrice, decimal QuoteMargin, BinanceFuturesOrder limitOrder, FuturesOrderType orderType)
    {
        this.OtoTaskCts = new CancellationTokenSource();
        this.OtoTaskStatus = OrderMonitoringTaskStatus.Unstarted;

        _ = this.OtoTaskAsync(limitOrder, orderType, LimitPrice, QuoteMargin);
    }
    private static void ValidateLimitOrderInput(OrderSide orderSide, decimal LimitPrice, decimal currentPrice, decimal? StopLoss = null, decimal? TakeProfit = null)
    {
        if (orderSide == OrderSide.Buy && LimitPrice > currentPrice)
            throw new InvalidOrderException("The limit price for a buy order can't be greater than the current price");
        
        if (orderSide == OrderSide.Sell && LimitPrice < currentPrice)
            throw new InvalidOrderException("The limit price for a sell order can't be less greater than the current price");
        
        ValidateTpSl(orderSide, LimitPrice, "limit price", StopLoss, TakeProfit);
    }
    private static void ValidateTpSl(OrderSide orderSide, decimal price, string priceType, decimal? stopLoss = null, decimal? takeProfit = null)
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

    public async Task<BinanceFuturesCancelOrder> CancelLimitOrderAsync()
    {
        if (this.LimitOrder is null)
            throw new InvalidOrderException("No limit order to cancel");


        var orderCancelResult = await this.FuturesApiService.CancelOrderAsync(this.CurrencyPair.Name, this.LimitOrder.Id);

        if (this.OtoTaskStatus == OrderMonitoringTaskStatus.Running)
            await this.CancelOtoAsync();
        
        if (this.OtocoTaskStatus == OrderMonitoringTaskStatus.Running)
            await this.CancelOtocoAsync();
        
        this.LimitOrder = null;
        return orderCancelResult;
    }
    private async Task CancelOtoAsync()
    {
        this.OtoTaskCts.Cancel();

        while (this.OtoTaskStatus == OrderMonitoringTaskStatus.Running)
            await Task.Delay(10);
    }
    private async Task CancelOtocoAsync()
    {
        this.OtocoTaskCts.Cancel();

        while (this.OtocoTaskStatus == OrderMonitoringTaskStatus.Running)
            await Task.Delay(10);
    }
    
    public async Task<BinanceFuturesOrder> PlaceStopLossAsync(decimal price)
    {
        if (this.Position is null)
        {
            throw new InvalidOrderException("No position is open thus a stop loss can't be placed");
        }


        var placeSlTask = this.FuturesApiService.PlaceOrderAsync(currencyPair: this.CurrencyPair.Name, side: this.Position.EntryOrder.Side.Invert(), type: FuturesOrderType.StopMarket, quantity: this.Position.EntryOrder.Quantity, stopPrice: Math.Round(price, 2), positionSide: this.Position.Side);

        if (this.Position.StopLossOrder is not null)
            await this.FuturesApiService.CancelOrderAsync(this.CurrencyPair.Name, this.Position.StopLossOrder.Id);


        var stopLossOrder = await placeSlTask;

        this.FireAndForgetOcoTask_IfTakeProfitAlreadyExisted(stopLossOrder);

        this.Position.StopLossOrder = stopLossOrder;
        return stopLossOrder;
    }
    private void FireAndForgetOcoTask_IfTakeProfitAlreadyExisted(BinanceFuturesOrder stopLossOrder)
    {
        this.OcoIDs ??= new StopLossTakeProfitIdPair();
        
        this.OcoIDs.StopLoss = stopLossOrder.Id;
        if (this.OcoIDs.HasBoth() && this.OcoTaskStatus == OrderMonitoringTaskStatus.Unstarted)
        {
            this.OcoTaskCts ??= new CancellationTokenSource();
            _ = this.OcoTaskAsync();
        }
    }
    
    public async Task<BinanceFuturesOrder> PlaceTakeProfitAsync(decimal price)
    {
        if (this.Position is null)
        {
            throw new InvalidOrderException("No position is open thus a take profit can't be placed");
        }


        var placeTpTask = this.FuturesApiService.PlaceOrderAsync(currencyPair: this.CurrencyPair.Name, side: this.Position.EntryOrder.Side.Invert(), type: FuturesOrderType.TakeProfitMarket, quantity: this.Position.EntryOrder.Quantity, stopPrice: Math.Round(price, 2), positionSide: this.Position.Side);

        if (this.Position.TakeProfitOrder is not null)
            await this.FuturesApiService.CancelOrderAsync(this.CurrencyPair.Name, this.Position.TakeProfitOrder.Id);

        var takeProfitOrder = await placeTpTask;

        this.FireAndForgetOcoTask_IfStopLossAlreadyExisted(takeProfitOrder);

        this.Position.TakeProfitOrder = takeProfitOrder;
        return takeProfitOrder;
    }
    private void FireAndForgetOcoTask_IfStopLossAlreadyExisted(BinanceFuturesOrder takeProfitOrder)
    {
        this.OcoIDs ??= new StopLossTakeProfitIdPair();
        
        this.OcoIDs.TakeProfit = takeProfitOrder.Id;
        if (this.OcoIDs.HasBoth() && this.OcoTaskStatus == OrderMonitoringTaskStatus.Unstarted)
        {
            this.OcoTaskCts ??= new CancellationTokenSource();
            _ = this.OcoTaskAsync();
        }
    }
    
    public async Task<BinanceFuturesOrder> ClosePositionAsync()
    {
        if (this.Position is null)
        {
            throw new InvalidOrderException($"No position is open", new NullReferenceException($"{nameof(this.Position)} is NULL"));
        }


        var positionClosingTask = this.FuturesApiService.PlaceOrderAsync(currencyPair: this.CurrencyPair.Name, side: this.Position.EntryOrder.Side.Invert(), type: FuturesOrderType.Market, quantity: this.Position.EntryOrder.Quantity, positionSide: this.Position.Side);
        await this.FuturesApiService.CancelOrdersAsync(this.CurrencyPair.Name, this.Position.GetOpenOrdersIDs().ToList());
        
        if (this.OcoTaskStatus == OrderMonitoringTaskStatus.Running)
            await this.CancelOcoAsync();

        this.Position = null;
        return await positionClosingTask;
    }
    private async Task CancelOcoAsync()
    {
        this.OcoTaskCts.Cancel();
        
        while (this.OcoTaskStatus == OrderMonitoringTaskStatus.Running)
            await Task.Delay(10);
        
        this.OcoIDs = null;
    }

    public bool IsInPosition() => this.Position is not null;

    
    private async Task OcoTaskAsync()
    {
        try
        {
            this.OcoTaskStatus = OrderMonitoringTaskStatus.Running;

            await this.OrderStatusMonitor.SubscribeToOrderUpdatesAsync();
            var filledOrderId = await this.OrderStatusMonitor.WaitForAnyOrderToReachStatusAsync(this.OcoIDs!.Values, OrderStatus.Filled, this.OcoTaskCts.Token);

            var orderToCancelID = this.OcoIDs.Values.Single(x => x != filledOrderId);
            await this.FuturesApiService.CancelOrderAsync(this.CurrencyPair.Name, orderToCancelID);
            
            this.OcoTaskStatus = OrderMonitoringTaskStatus.Completed;
        }
        catch (OperationCanceledException) { this.OcoTaskStatus = OrderMonitoringTaskStatus.Cancelled; }
        catch { this.OcoTaskStatus = OrderMonitoringTaskStatus.Faulted; }
    }
    public async Task OtoTaskAsync(BinanceFuturesOrder limitOrder, FuturesOrderType orderType, decimal limitPrice, decimal Margin)
    {
        try
        {
            this.OtoTaskStatus = OrderMonitoringTaskStatus.Running;

            await this.OrderStatusMonitor.SubscribeToOrderUpdatesAsync();
            await this.OrderStatusMonitor.WaitForOrderToReachStatusAsync(limitOrder.Id, OrderStatus.Filled, this.OtoTaskCts.Token);

            
            var task = this.FuturesApiService.PlaceOrderAsync(currencyPair: this.CurrencyPair.Name, side: limitOrder.Side.Invert(), type: orderType, quantity: 0, stopPrice: limitPrice, positionSide: limitOrder.PositionSide, timeInForce: TimeInForce.GoodTillCanceled, closePosition: true);
            
            var entryOrder = await this.MarketDataProvider.GetOrderAsync(this.CurrencyPair.Name, limitOrder.Id);
            var newLimitOrder = await task;

            this.Position = new FuturesPosition
            {
                CurrencyPair = this.CurrencyPair,

                Leverage = this.Leverage,
                Margin = Margin,

                EntryOrder = entryOrder,
                StopLossOrder = newLimitOrder.Type == FuturesOrderType.StopMarket ? newLimitOrder : null,
                TakeProfitOrder = newLimitOrder.Type == FuturesOrderType.TakeProfitMarket ? newLimitOrder : null,
            };

            this.OtoTaskStatus = OrderMonitoringTaskStatus.Completed;
        }
        catch (OperationCanceledException) { this.OtoTaskStatus = OrderMonitoringTaskStatus.Cancelled; }
        catch { this.OtoTaskStatus = OrderMonitoringTaskStatus.Faulted; }
    }
    public async Task OtocoTaskAsync(BinanceFuturesOrder limitOrder, decimal StopLoss, decimal TakeProfit, decimal Margin)
    {
        try
        {
            this.OtocoTaskStatus = OrderMonitoringTaskStatus.Running;

            await this.OrderStatusMonitor.SubscribeToOrderUpdatesAsync();
            await this.OrderStatusMonitor.WaitForOrderToReachStatusAsync(limitOrder.Id, OrderStatus.Filled, this.OtocoTaskCts.Token);

            // limitOrder was filled ==> OTOCO turned into OCO

            var positionSide = limitOrder.PositionSide;
            var inverseOrderSide = limitOrder.Side.Invert();

            var getEntryOrderTask = this.MarketDataProvider.GetOrderAsync(this.CurrencyPair.Name, limitOrder.Id);
            var placeStopLossTask = this.FuturesApiService.PlaceOrderAsync(currencyPair: this.CurrencyPair.Name, side: inverseOrderSide, type: FuturesOrderType.StopMarket, quantity: 0, stopPrice: StopLoss, positionSide: positionSide, timeInForce: TimeInForce.GoodTillCanceled, closePosition: true);
            var placeTakeProfitTask = this.FuturesApiService.PlaceOrderAsync(currencyPair: this.CurrencyPair.Name, side: inverseOrderSide, type: FuturesOrderType.TakeProfitMarket, quantity: 0, stopPrice: TakeProfit, positionSide: positionSide, timeInForce: TimeInForce.GoodTillCanceled, closePosition: true);

            var entryOrder = await getEntryOrderTask;
            var stopLossOrder = await placeStopLossTask;
            var takeProfitOrder = await placeTakeProfitTask;
            
            this.Position = new FuturesPosition
            {
                CurrencyPair = this.CurrencyPair,

                Leverage = this.Leverage,
                Margin = Margin,

                EntryOrder = entryOrder,
                StopLossOrder = stopLossOrder,
                TakeProfitOrder = takeProfitOrder,
            };
            
            this.OtocoTaskStatus = OrderMonitoringTaskStatus.Completed;
        }
        catch (OperationCanceledException) { this.OtocoTaskStatus = OrderMonitoringTaskStatus.Cancelled; }
        catch { this.OtocoTaskStatus = OrderMonitoringTaskStatus.Faulted; }


        if (this.OtocoTaskStatus is OrderMonitoringTaskStatus.Cancelled or OrderMonitoringTaskStatus.Faulted)
            return;

        // this.Position is not null ==> safe to use OCO with this.OcoTaskCts and this.OcoIDs
        // OcoTaskCts.Cancel() will be called if the position is closed
        
        this.OcoTaskCts = new CancellationTokenSource();
        this.OcoIDs = new StopLossTakeProfitIdPair();
        this.OcoTaskStatus = OrderMonitoringTaskStatus.Unstarted;

        this.OcoIDs.StopLoss = this.Position!.StopLossOrder!.Id;
        this.OcoIDs.TakeProfit = this.Position!.TakeProfitOrder!.Id;
        
        await this.OcoTaskAsync();
    }

    
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
