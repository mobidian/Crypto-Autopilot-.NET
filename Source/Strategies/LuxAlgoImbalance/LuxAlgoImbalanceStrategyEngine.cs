using Application.Extensions.Bybit;
using Application.Interfaces.Services.Bybit;
using Application.Interfaces.Services.Bybit.Monitors;
using Application.Strategies;

using Bybit.Net.Enums;

using Domain.Models.Common;

using Strategies.LuxAlgoImbalance.Enums;
using Strategies.LuxAlgoImbalance.Interfaces.Services;
using Strategies.LuxAlgoImbalance.Services;

namespace Strategies.LuxAlgoImbalance;

public class LuxAlgoImbalanceStrategyEngine : StrategyEngine
{
    private readonly CurrencyPair CurrencyPair;
    private readonly KlineInterval Timeframe;

    public LuxAlgoImbalanceStrategyEngine(CurrencyPair currencyPair, KlineInterval timeframe, IBybitUsdFuturesMarketDataProvider marketDataProvider, IBybitUsdPerpetualKlinesMonitor klinesMonitor, IBybitFuturesAccountDataProvider futuresAccount, IBybitUsdFuturesTradingService tradingService) : this(currencyPair, timeframe, marketDataProvider, klinesMonitor, futuresAccount, tradingService, new FairValueGapFinder()) { }
    internal LuxAlgoImbalanceStrategyEngine(CurrencyPair currencyPair, KlineInterval timeframe, IBybitUsdFuturesMarketDataProvider marketDataProvider, IBybitUsdPerpetualKlinesMonitor klinesMonitor, IBybitFuturesAccountDataProvider futuresAccount, IBybitUsdFuturesTradingService tradingService, IFairValueGapFinder fvgFinder) : base(marketDataProvider, klinesMonitor, futuresAccount, tradingService)
    {
        this.CurrencyPair = currencyPair;
        this.Timeframe = timeframe;
        this.FvgFinder = fvgFinder;
    }


    private IEnumerable<Candlestick> Candlesticks = default!;
    private IFairValueGapFinder FvgFinder;
    private bool? FvgFormed;


    protected override async Task TakeActionAsync()
    {
        await this.KlinesMonitor.WaitForNextCandlestickAsync(this.CurrencyPair.Name, this.Timeframe);
        this.Candlesticks = (await this.MarketDataProvider.GetCompletedCandlesticksAsync(this.CurrencyPair.Name, this.Timeframe)).ToCandlesticks();

        if (this.FvgFormed is not null) // UNDONE: need to set take profit as well
        {
            var luxAlgoFVG = this.FvgFinder.FindLast(this.Candlesticks)!.Value;
            var orderSide = luxAlgoFVG.Side == FvgSide.Bullish ? OrderSide.Buy : OrderSide.Sell;

            var margin = (await this.FuturesAccount.GetAssetBalanceAsync(this.CurrencyPair.Name)).AvailableBalance * 0.99m;
            var limitPrice = luxAlgoFVG.Middle;
            var stoploss = orderSide == OrderSide.Buy ? luxAlgoFVG.Bottom : luxAlgoFVG.Top;

            await this.TradingService.PlaceLimitOrderAsync(orderSide, limitPrice, margin, stoploss);

            this.FvgFormed = null; // the signal has been consumed
        }

        if (this.TradingService.LongPosition is not null)
            ; // // TODO stop loss checks/updates

        if (this.TradingService.ShortPosition is not null)
            ; // // TODO stop loss checks/updates
    }


    /// <summary>
    /// Used to notify the <see cref="LuxAlgoImbalanceStrategyEngine"/> that a new Fair Value Gap has been formed
    /// </summary>
    public void SignalFairValueGap() => this.FvgFormed = true;
}
