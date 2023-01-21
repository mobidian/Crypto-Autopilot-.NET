using Infrastructure.Strategies.SimpleStrategy;
using Infrastructure.Tests.Unit.SimpleStrategyEngineTests.Base;

namespace Infrastructure.Tests.Unit.SimpleStrategyEngineTests.LongStrategy;

public abstract class LongStrategyEngineTestsBase : SimpleStrategyEngineTestsBase
{
    public LongStrategyEngineTestsBase() : base()
    {
        this.SUT = new SimpleLongStrategyEngine(this.CurrencyPair, this.KlineInterval, this.FuturesTrader, this.FuturesDataProvider, this.CandlestickAwaiter, this.Mediator);
        this.Candlesticks = this.CandlestickGenerator.Generate(100);
    }
}
