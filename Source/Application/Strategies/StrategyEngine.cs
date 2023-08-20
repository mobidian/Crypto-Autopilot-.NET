using Application.Interfaces.Services.Bybit;
using Application.Interfaces.Services.Bybit.Monitors;
using Application.Interfaces.Services.General;

using MediatR;

namespace Application.Strategies;

/// <summary>
/// <para>Implements the <see cref="IStrategyEngine"/> interface and provides an abstract base class for implementing specific trading strategies.</para>
/// <para>It contains the basic structure and methods for managing the lifecycle of a trading strategy.</para>
/// </summary>
public abstract class StrategyEngine : IStrategyEngine
{
    public Guid Guid { get; } = Guid.NewGuid();

    private readonly CancellationTokenSource CTS;
    protected readonly IDateTimeProvider DateTimeProvider;
    protected readonly IBybitUsdFuturesMarketDataProvider MarketDataProvider;
    protected readonly IBybitUsdPerpetualKlinesMonitor KlinesMonitor;
    protected readonly IBybitFuturesAccountDataProvider FuturesAccount;
    protected readonly IBybitUsdFuturesTradingService TradingService;
    protected readonly IMediator Mediator;

    protected StrategyEngine(IDateTimeProvider dateTimeProvider, IBybitUsdFuturesMarketDataProvider marketDataProvider, IBybitUsdPerpetualKlinesMonitor klinesMonitor, IBybitFuturesAccountDataProvider futuresAccount, IBybitUsdFuturesTradingService tradingService, IMediator mediator)
    {
        this.CTS = new CancellationTokenSource();
        this.DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        this.MarketDataProvider = marketDataProvider ?? throw new ArgumentNullException(nameof(marketDataProvider));
        this.KlinesMonitor = klinesMonitor ?? throw new ArgumentNullException(nameof(klinesMonitor));
        this.FuturesAccount = futuresAccount ?? throw new ArgumentNullException(nameof(futuresAccount));
        this.TradingService = tradingService ?? throw new ArgumentNullException(nameof(tradingService));
        this.Mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }


    //// //// ////


    private volatile bool Running;
    public bool IsRunning() => this.Running;

    public async Task StartTradingAsync()
    {
        this.Running = true;

        while (!this.CTS.IsCancellationRequested)
            await this.TakeActionAsync();

        await Task.WhenAll(this.TradingService.CloseAllPositionsAsync(),
                           this.TradingService.CancelAllLimitOrdersAsync());

        this.Running = false;
    }
    protected abstract Task TakeActionAsync();

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
        this.Dispose(disposing: true);
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
        await this.DisposeAsync(disposing: true);
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
