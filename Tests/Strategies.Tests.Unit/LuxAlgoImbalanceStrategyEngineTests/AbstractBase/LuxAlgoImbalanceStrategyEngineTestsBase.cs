using Application.Interfaces.Services.Bybit;
using Application.Interfaces.Services.Bybit.Monitors;
using Application.Interfaces.Services.General;

using Bogus;

using Bybit.Net.Enums;
using Bybit.Net.Objects.Models;

using Domain.Models.Common;

using MediatR;

using NSubstitute;

using Strategies.LuxAlgoImbalance;
using Strategies.LuxAlgoImbalance.Interfaces.Services;

namespace Strategies.Tests.Unit.LuxAlgoImbalanceStrategyEngineTests.AbstractBase;

public abstract class LuxAlgoImbalanceStrategyEngineTestsBase
{
    protected readonly LuxAlgoImbalanceStrategyEngine SUT;
    protected readonly CurrencyPair CurrencyPair;
    protected readonly KlineInterval Timeframe;
    protected readonly IDateTimeProvider DateTimeProvider;
    protected readonly IBybitUsdFuturesMarketDataProvider MarketDataProvider;
    protected readonly IBybitUsdPerpetualKlinesMonitor KlinesMonitor;
    protected readonly IBybitFuturesAccountDataProvider FuturesAccount;
    protected readonly IBybitUsdFuturesTradingService TradingService;
    protected readonly IMediator Mediator;
    protected readonly IFairValueGapFinder FvgFinder;

    #region Candlesticks/Klines fakers
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
    #endregion

    public LuxAlgoImbalanceStrategyEngineTestsBase()
    {
        this.CurrencyPair = new CurrencyPair("BTC", "USDT");
        this.Timeframe = KlineInterval.OneHour;
        this.DateTimeProvider = Substitute.For<IDateTimeProvider>();
        this.MarketDataProvider = Substitute.For<IBybitUsdFuturesMarketDataProvider>();
        this.KlinesMonitor = Substitute.For<IBybitUsdPerpetualKlinesMonitor>();
        this.FuturesAccount = Substitute.For<IBybitFuturesAccountDataProvider>();
        this.TradingService = Substitute.For<IBybitUsdFuturesTradingService>();
        this.FvgFinder = Substitute.For<IFairValueGapFinder>();
        this.Mediator = Substitute.For<IMediator>();

        this.SUT = new LuxAlgoImbalanceStrategyEngine(this.CurrencyPair, this.Timeframe, this.DateTimeProvider, this.MarketDataProvider, this.KlinesMonitor, this.FuturesAccount, this.TradingService, this.Mediator, this.FvgFinder);
    }
}
