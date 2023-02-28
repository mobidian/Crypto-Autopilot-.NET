using Application.Interfaces.Services.Trading;
using Application.Interfaces.Services.Trading.Monitors;

using Binance.Net.Enums;

using Domain.Models;

using Infrastructure.Services.Trading.Strategies.SimpleStrategy;

using MediatR;

namespace Infrastructure.Tests.Unit.SimpleStrategyEngineTests.Base;

public abstract class SimpleStrategyEngineTestsBase
{
    protected readonly Random Random = new Random();

    protected readonly Faker<Candlestick> CandlestickGenerator = new Faker<Candlestick>()
        .RuleFor(x => x.CurrencyPair, f => new CurrencyPair(f.Finance.Currency().Code, f.Finance.Currency().Code))
        .RuleFor(c => c.Date, f => f.Date.Recent(365))
        .RuleFor(c => c.Open, f => f.Random.Decimal(1000, 1500))
        .RuleFor(c => c.High, (f, c) => f.Random.Decimal(c.Open, c.Open + 100))
        .RuleFor(c => c.Low, (f, c) => f.Random.Decimal(c.Open - 100, c.Open))
        .RuleFor(c => c.Close, (f, c) => f.Random.Decimal(1000, 1500))
        .RuleFor(c => c.Volume, f => f.Random.Decimal(100000, 300000));

    protected IReadOnlyList<Candlestick> Candlesticks = default!;
    
    protected SimpleStrategyEngine SUT = default!;
    protected decimal Margin;
    protected decimal StopLossParameter;
    protected decimal TakeProfitParameter;

    protected readonly CurrencyPair CurrencyPair = new CurrencyPair("ETH", "BUSD");
    protected readonly KlineInterval KlineInterval = KlineInterval.FifteenMinutes;

    protected readonly ICfdTradingService FuturesTrader = Substitute.For<ICfdTradingService>();
    protected readonly ICfdMarketDataProvider FuturesDataProvider = Substitute.For<ICfdMarketDataProvider>();
    protected readonly IFuturesCandlesticksMonitor CandlestickMonitor = Substitute.For<IFuturesCandlesticksMonitor>();
    protected readonly IMediator Mediator = Substitute.For<IMediator>();
}
