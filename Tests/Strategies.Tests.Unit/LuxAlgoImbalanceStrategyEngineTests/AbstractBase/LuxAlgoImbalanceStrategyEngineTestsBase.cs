using Application.Interfaces.Services.Bybit;
using Application.Interfaces.Services.Bybit.Monitors;

using Bogus;

using Bybit.Net.Enums;
using Bybit.Net.Objects.Models;

using Domain.Models.Common;

using NSubstitute;

using Strategies.LuxAlgoImbalance;

namespace Strategies.Tests.Unit.LuxAlgoImbalanceStrategyEngineTests.AbstractBase;

public abstract class LuxAlgoImbalanceStrategyEngineTestsBase
{
    protected LuxAlgoImbalanceStrategyEngine SUT;
    protected CurrencyPair CurrencyPair;
    protected KlineInterval Timeframe;
    protected IBybitUsdFuturesMarketDataProvider MarketDataProvider;
    protected IBybitUsdPerpetualKlinesMonitor KlinesMonitor;
    protected IBybitFuturesAccountDataProvider FuturesAccount;
    protected IBybitUsdFuturesTradingService TradingService;
    protected IFairValueGapFinder FvgFinder;

    protected Faker<Candlestick> CandlesticksFaker = new Faker<Candlestick>()
        .RuleFor(x => x.CurrencyPair, f => new CurrencyPair("BTC", "USDT"))
        .RuleFor(x => x.Date, f => f.Date.Past())
        .RuleFor(x => x.Open, f => f.Random.Decimal(1000, 2000))
        .RuleFor(x => x.High, f => f.Random.Decimal(2000, 3000))
        .RuleFor(x => x.Close, f => f.Random.Decimal(500, 1000))
        .RuleFor(x => x.Low, f => f.Random.Decimal(1500, 2500))
        .RuleFor(x => x.Volume, f => f.Random.Decimal(1000000, 2000000));

    protected Faker<BybitKline> BybitKlinesFaker = new Faker<BybitKline>()
        .RuleFor(x => x.Symbol, f => "BTCUSDT")
        .RuleFor(x => x.Interval, KlineInterval.OneHour)
        .RuleFor(x => x.OpenPrice, f => f.Random.Decimal(1000, 2000))
        .RuleFor(x => x.HighPrice, f => f.Random.Decimal(2000, 3000))
        .RuleFor(x => x.ClosePrice, f => f.Random.Decimal(500, 1000))
        .RuleFor(x => x.LowPrice, f => f.Random.Decimal(1500, 2500))
        .RuleFor(x => x.Volume, f => f.Random.Decimal(1000000, 2000000));

    public LuxAlgoImbalanceStrategyEngineTestsBase()
    {
        this.CurrencyPair = new CurrencyPair("BTC", "USDT");
        this.Timeframe = KlineInterval.OneHour;
        this.MarketDataProvider = Substitute.For<IBybitUsdFuturesMarketDataProvider>();
        this.KlinesMonitor = Substitute.For<IBybitUsdPerpetualKlinesMonitor>();
        this.FuturesAccount = Substitute.For<IBybitFuturesAccountDataProvider>();
        this.TradingService = Substitute.For<IBybitUsdFuturesTradingService>();
        this.FvgFinder = Substitute.For<IFairValueGapFinder>();

        this.SUT = new LuxAlgoImbalanceStrategyEngine(this.CurrencyPair, this.Timeframe, this.MarketDataProvider, this.KlinesMonitor, this.FuturesAccount, this.TradingService, this.FvgFinder);
    }
}
