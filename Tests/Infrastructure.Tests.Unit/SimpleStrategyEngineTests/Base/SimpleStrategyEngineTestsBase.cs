using Application.Interfaces.Services.Trading;

using Binance.Net.Enums;

using Domain.Models;

using Infrastructure.Services.Trading;
using Infrastructure.Strategies.SimpleStrategy;

using MediatR;

namespace Infrastructure.Tests.Unit.SimpleStrategyEngineTests.Base;

public abstract class SimpleStrategyEngineTestsBase
{
    protected readonly Faker<Candlestick> CandlestickGenerator = new Faker<Candlestick>()
        .RuleFor(x => x.CurrencyPair, f => new CurrencyPair(f.Finance.Currency().Code, f.Finance.Currency().Code))
        .RuleFor(c => c.Date, f => f.Date.Recent(365))
        .RuleFor(c => c.Open, f => f.Random.Decimal(1000, 1500))
        .RuleFor(c => c.High, (f, c) => f.Random.Decimal(c.Open, c.Open + 100))
        .RuleFor(c => c.Low, (f, c) => f.Random.Decimal(c.Open - 100, c.Open))
        .RuleFor(c => c.Close, (f, c) => f.Random.Decimal(1000, 1500))
        .RuleFor(c => c.Volume, f => f.Random.Decimal(100000, 300000));

    protected readonly IReadOnlyList<Candlestick> Candlesticks;



    protected readonly SimpleStrategyEngine SUT;

    protected readonly CurrencyPair CurrencyPair = new CurrencyPair("ETH", "BUSD");
    protected readonly KlineInterval KlineInterval = KlineInterval.FifteenMinutes;

    protected readonly ICfdTradingService FuturesTrader = Substitute.For<ICfdTradingService>();
    protected readonly ICfdMarketDataProvider FuturesDataProvider = Substitute.For<ICfdMarketDataProvider>();
    protected readonly IFuturesMarketsCandlestickAwaiter CandlestickAwaiter = Substitute.For<IFuturesMarketsCandlestickAwaiter>();
    protected readonly IMediator Mediator = Substitute.For<IMediator>();
    
    public SimpleStrategyEngineTestsBase()
    {
        this.SUT = new SimpleStrategyEngine(this.CurrencyPair, this.KlineInterval, this.FuturesTrader, this.FuturesDataProvider, this.CandlestickAwaiter, this.Mediator);
        this.Candlesticks = this.CandlestickGenerator.Generate(100);
    } 
}
