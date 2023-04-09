using Application.Interfaces.Services;
using Application.Interfaces.Services.Bybit;
using Application.Interfaces.Services.Bybit.Monitors;

using Bybit.Net.Enums;

namespace Infrastructure.Services.Trading;

/// <summary>
/// <para>The StrategyEngine class provides an abstract base class for implementing specific trading strategies.</para>
/// <para>It contains the basic structure and methods for managing the lifecycle of a trading strategy.</para>
/// </summary>
public abstract class StrategyEngine : IStrategyEngine
{
    public Guid Guid { get; } = Guid.NewGuid();

    private readonly CancellationTokenSource CTS;
    protected readonly IBybitUsdFuturesMarketDataProvider MarketDataProvider;
    protected readonly IBybitUsdPerpetualKlinesMonitor KlinesMonitor;
    protected readonly IBybitFuturesAccountDataProvider FuturesAccount;
    protected readonly IBybitUsdFuturesTradingService TradingService;
    
    protected StrategyEngine(IBybitUsdFuturesMarketDataProvider marketDataProvider, IBybitUsdPerpetualKlinesMonitor klinesMonitor, IBybitFuturesAccountDataProvider futuresAccount, IBybitUsdFuturesTradingService tradingService)
    {
        this.CTS = new CancellationTokenSource();
        this.MarketDataProvider = marketDataProvider ?? throw new ArgumentNullException(nameof(marketDataProvider));
        this.KlinesMonitor = klinesMonitor ?? throw new ArgumentNullException(nameof(klinesMonitor));
        this.FuturesAccount = futuresAccount ?? throw new ArgumentNullException(nameof(futuresAccount));
        this.TradingService = tradingService ?? throw new ArgumentNullException(nameof(tradingService));
    }


    //// //// ////


    private volatile bool Running;
    public bool IsRunning() => this.Running;

    public async Task StartTradingAsync()
    {
        this.Running = true;

        while (!this.CTS.IsCancellationRequested)
            await this.TakeActionAsync();
        
        await this.ClosePositionsAndLimitOrdersAsync();

        this.Running = false;
    }
    protected abstract Task TakeActionAsync();
    private async Task ClosePositionsAndLimitOrdersAsync()
    {
        var tasks = new List<Task>();

        if (this.TradingService.LongPosition is not null)
            tasks.Add(this.TradingService.ClosePositionAsync(PositionSide.Buy));

        if (this.TradingService.ShortPosition is not null)
            tasks.Add(this.TradingService.ClosePositionAsync(PositionSide.Sell));
        
        if (this.TradingService.BuyLimitOrder is not null)
            tasks.Add(this.TradingService.CancelLimitOrderAsync(OrderSide.Buy));
        
        if (this.TradingService.SellLimitOrder is not null)
            tasks.Add(this.TradingService.CancelLimitOrderAsync(OrderSide.Sell));
        
        await Task.WhenAll(tasks);
    }

    public async Task StopTradingAsync()
    {
        if (!this.CTS.IsCancellationRequested)
            this.CTS.Cancel();
        
        while (this.Running)
            await Task.Delay(20);
    }


    //// //// ////


    private bool Disposed;

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (this.Disposed)
            return;

        if (disposing)
        {
            this.CTS.Dispose();
            this.StopTradingAsync().GetAwaiter().GetResult();
        }

        this.Disposed = true;
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(disposing: true);
        GC.SuppressFinalize(this);
    }
    protected virtual async Task DisposeAsync(bool disposing)
    {
        if (this.Disposed)
            return;
        
        if (disposing)
        {
            this.CTS.Dispose();
            await this.StopTradingAsync();
        }
        
        this.Disposed = true;
    }
}
