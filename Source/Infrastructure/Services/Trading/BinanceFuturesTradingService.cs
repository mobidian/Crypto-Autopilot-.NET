using Application.Interfaces.Services.Trading;
using Application.Interfaces.Services.Trading.Monitors;

using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;

using Domain.Extensions;
using Domain.Models;

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
    private CancellationToken OcoTaskCancellationToken;
    internal StopLossTakeProfitIDs OcoIDs = default!;
    internal Task? OcoTask;
    
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
        
        this.FireAndForgetOcoTaskIfSlTpArePlaced();
        
        return ordersArray;
    }
    private void FireAndForgetOcoTaskIfSlTpArePlaced()
    {
        this.OcoTaskCts = new CancellationTokenSource();
        this.OcoTaskCancellationToken = this.OcoTaskCts.Token;
        this.OcoIDs = new StopLossTakeProfitIDs();
        
        if (this.Position!.StopLossOrder is not null)
            this.OcoIDs.StopLoss = this.Position!.StopLossOrder.Id;
        
        if (this.Position.TakeProfitOrder is not null)
            this.OcoIDs.TakeProfit = this.Position.TakeProfitOrder.Id;
        
        if (this.OcoIDs.HasBothStopLossAndTakeProfit())
            this.OcoTask = this.OcoMonitoringTaskAsync();
    }

    public async Task<BinanceFuturesOrder> PlaceLimitOrderAsync(OrderSide OrderSide, decimal LimitPrice, decimal QuoteMargin = decimal.MaxValue, decimal? StopLoss = null, decimal? TakeProfit = null)
    {
        if (this.IsInPosition())
        {
            throw new InvalidOperationException("A position is open already");
        }
        
        
        return await this.FuturesApiService.PlaceLimitOrderAsync(this.CurrencyPair.Name, OrderSide, LimitPrice, QuoteMargin, this.Leverage, StopLoss, TakeProfit);
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
        
        this.OcoIDs.StopLoss = stopLossOrder.Id;
        if (this.OcoIDs.HasBothStopLossAndTakeProfit() && this.OcoTask is not null)
            this.OcoTask = this.OcoMonitoringTaskAsync();

        this.Position.StopLossOrder = stopLossOrder;
        return stopLossOrder;
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
        
        this.OcoIDs.TakeProfit = takeProfitOrder.Id;
        if (this.OcoIDs.HasBothStopLossAndTakeProfit() && this.OcoTask is not null)
            this.OcoTask = this.OcoMonitoringTaskAsync();

        this.Position.TakeProfitOrder = takeProfitOrder;
        return takeProfitOrder;
    }

    public async Task<BinanceFuturesOrder> ClosePositionAsync()
    {
        if (this.Position is null)
        {
            throw new InvalidOperationException($"No position is open", new NullReferenceException($"{nameof(this.Position)} is NULL"));
        }


        var positionClosingTask = this.FuturesApiService.PlaceOrderAsync(currencyPair: this.CurrencyPair.Name, side: this.Position.EntryOrder.Side.Invert(), type: FuturesOrderType.Market, quantity: this.Position.EntryOrder.Quantity, positionSide: this.Position.Side);
        await this.FuturesApiService.CancelOrdersAsync(this.CurrencyPair.Name, this.Position.GetOpenOrdersIDs().ToList());

        await this.CancelOcoAndResetOcoTaskCts();
        
        this.OcoIDs = null!;
        this.Position = null;
        return await positionClosingTask;
    }
    private async Task CancelOcoAndResetOcoTaskCts()
    {
        this.OcoTaskCts.Cancel();
        await Task.Delay(50);
        
        this.OcoTaskCts.Dispose();
        
        this.OcoTask = null!;
    }

    public bool IsInPosition() => this.Position is not null;
    

    
    private async Task OcoMonitoringTaskAsync()
    {
        try
        {
            await this.OrderStatusMonitor.SubscribeToOrderUpdatesAsync();
            var filledOrderId = await this.OrderStatusMonitor.WaitForAnyOrderToReachStatusAsync(this.OcoIDs.Values, OrderStatus.Filled, this.OcoTaskCancellationToken);
            
            var orderToCancelID = this.OcoIDs.Values.Single(x => x != filledOrderId);
            await this.FuturesApiService.CancelOrderAsync(this.CurrencyPair.Name, orderToCancelID);
        }
        catch (OperationCanceledException) {  }
        catch (Exception)
        {
            // // TODO handle exception from fire and forget // //
        }
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


    //// //// ////

    
    internal class StopLossTakeProfitIDs
    {
        public long[] Values { get; } = { 0, 0 };
        
        public long StopLoss
        {
            get => Values[0];
            set => Values[0] = value;
        }

        public long TakeProfit
        {
            get => Values[1];
            set => Values[1] = value;
        }

        public bool HasBothStopLossAndTakeProfit() => !this.Values.Contains(0);
    }
}
