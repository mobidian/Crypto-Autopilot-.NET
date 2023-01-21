using Infrastructure.Strategies.SimpleStrategy;
using Infrastructure.Tests.Unit.SimpleStrategyEngineTests.Base;

namespace Infrastructure.Tests.Unit.SimpleStrategyEngineTests.ShortStrategy;

public class ShortStrategyEngineTestsBase : SimpleStrategyEngineTestsBase
{
    public ShortStrategyEngineTestsBase() : base()
    {
        this.SUT = new SimpleShortStrategyEngine(this.CurrencyPair, this.KlineInterval, this.FuturesTrader, this.FuturesDataProvider, this.CandlestickAwaiter, this.Mediator);
        this.Candlesticks = this.CandlestickGenerator.Generate(100);
    }
}
