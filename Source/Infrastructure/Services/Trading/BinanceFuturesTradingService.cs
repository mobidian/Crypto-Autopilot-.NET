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
            throw new InvalidOperationException("A position is open already");
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
        
        this.FireAndForgetOcoTask_IfStopLossAndTakeProfitArePlaced();
        
        return ordersArray;
    }
    private void FireAndForgetOcoTask_IfStopLossAndTakeProfitArePlaced()
    {
        this.OcoTaskCts = new CancellationTokenSource();
        this.OcoIDs = new StopLossTakeProfitIdPair();
        this.OcoTaskStatus = OrderMonitoringTaskStatus.Unstarted;

        if (this.Position!.StopLossOrder is not null)
            this.OcoIDs.StopLoss = this.Position!.StopLossOrder.Id;
        
        if (this.Position.TakeProfitOrder is not null)
            this.OcoIDs.TakeProfit = this.Position.TakeProfitOrder.Id;
        
        if (this.OcoIDs.HasBoth())
            _ = this.OcoMonitoringTaskAsync();
    }
    
    public async Task<BinanceFuturesOrder> PlaceLimitOrderAsync(OrderSide OrderSide, decimal LimitPrice, decimal QuoteMargin = decimal.MaxValue, decimal? StopLoss = null, decimal? TakeProfit = null)
    {
        if (this.IsInPosition())
        {
            throw new InvalidOperationException("A position is open already");
        }

        
        var currentPrice = await this.MarketDataProvider.GetCurrentPriceAsync(this.CurrencyPair.Name);
        var Quantity = Math.Round(QuoteMargin * this.Leverage / currentPrice, 3);
        var limitOrder = await this.FuturesApiService.PlaceOrderAsync(this.CurrencyPair.Name, OrderSide, FuturesOrderType.Limit, Quantity, LimitPrice, OrderSide.ToPositionSide(), TimeInForce.GoodTillCanceled);

        if (StopLoss is not null && TakeProfit is not null)
            _ = this.OtocoTaskAsync(limitOrder, StopLoss.Value, TakeProfit.Value, QuoteMargin);

        var orderType = StopLoss is not null ? FuturesOrderType.StopMarket : FuturesOrderType.TakeProfitMarket;
        _ = this.OtoTaskAsync(limitOrder, orderType, LimitPrice, QuoteMargin);
        
        return limitOrder;
    }
    public async Task OtoTaskAsync(BinanceFuturesOrder limitOrder, FuturesOrderType orderType, decimal limitPrice, decimal Margin) 
    {
        try
        {
            this.OtoTaskStatus = OrderMonitoringTaskStatus.Running;
             
            await this.OrderStatusMonitor.SubscribeToOrderUpdatesAsync();
            await this.OrderStatusMonitor.WaitForOrderToReachStatusAsync(limitOrder.Id, OrderStatus.Filled, this.OtoTaskCts.Token);

            
            var task = this.FuturesApiService.PlaceOrderAsync(this.CurrencyPair.Name, limitOrder.Side.Invert(), orderType, 0, limitPrice, limitOrder.PositionSide, TimeInForce.GoodTillCanceled, closePosition: true);
            
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
            var inverseOrderSide = limitOrder.Side;

            var getEntryOrderTask = this.MarketDataProvider.GetOrderAsync(this.CurrencyPair.Name, limitOrder.Id);
            var placeStopLossTask = this.FuturesApiService.PlaceOrderAsync(this.CurrencyPair.Name, inverseOrderSide, FuturesOrderType.StopMarket, 0, StopLoss, positionSide, TimeInForce.GoodTillCanceled, closePosition: true);
            var placeTakeProfitTask = this.FuturesApiService.PlaceOrderAsync(this.CurrencyPair.Name, inverseOrderSide, FuturesOrderType.TakeProfitMarket, 0, TakeProfit, positionSide, TimeInForce.GoodTillCanceled, closePosition: true);

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

        _ = this.OcoMonitoringTaskAsync();
    }
    
    public async Task<BinanceFuturesOrder> PlaceStopLossAsync(decimal price)
    {
        if (this.Position is null)
        {
            throw new InvalidOperationException("No position is open thus a stop loss can't be placed");
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
            _ = this.OcoMonitoringTaskAsync();
    }
    
    public async Task<BinanceFuturesOrder> PlaceTakeProfitAsync(decimal price)
    {
        if (this.Position is null)
        {
            throw new InvalidOperationException("No position is open thus a take profit can't be placed");
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
            _ = this.OcoMonitoringTaskAsync();
    }
    
    public async Task<BinanceFuturesOrder> ClosePositionAsync()
    {
        if (this.Position is null)
        {
            throw new InvalidOperationException($"No position is open", new NullReferenceException($"{nameof(this.Position)} is NULL"));
        }


        var positionClosingTask = this.FuturesApiService.PlaceOrderAsync(currencyPair: this.CurrencyPair.Name, side: this.Position.EntryOrder.Side.Invert(), type: FuturesOrderType.Market, quantity: this.Position.EntryOrder.Quantity, positionSide: this.Position.Side);
        await this.FuturesApiService.CancelOrdersAsync(this.CurrencyPair.Name, this.Position.GetOpenOrdersIDs().ToList());

        await this.CancelOcoTaskAndResetOcoTaskCts();
        
        this.Position = null;
        return await positionClosingTask;
    }
    private async Task CancelOcoTaskAndResetOcoTaskCts()
    {
        if (this.OcoTaskStatus != OrderMonitoringTaskStatus.Running)
            return;

        this.OcoTaskCts.Cancel();
        
        while (this.OcoTaskStatus == OrderMonitoringTaskStatus.Running)
            await Task.Delay(10);
        
        this.OcoIDs = null;
    }

    public bool IsInPosition() => this.Position is not null;
    

    
    private async Task OcoMonitoringTaskAsync()
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
