using Infrastructure.Services.Trading.Strategies.SimpleStrategy;
using Infrastructure.Tests.Unit.SimpleStrategyEngineTests.Base;

namespace Infrastructure.Tests.Unit.SimpleStrategyEngineTests.LongStrategy;

public abstract class LongStrategyEngineTestsBase : SimpleStrategyEngineTestsBase
{
    public LongStrategyEngineTestsBase() : base()
    {
        this.Margin = this.Random.Next(100, 1000);
        this.StopLossParameter = Convert.ToDecimal(this.Random.NextDouble());
        this.TakeProfitParameter = Convert.ToDecimal(this.Random.Next(1, 10) + this.Random.NextDouble());
        
        this.SUT = new SimpleLongStrategyEngine(
            this.CurrencyPair,
            this.KlineInterval,
            this.Margin,
            this.StopLossParameter,
            this.TakeProfitParameter,
            this.FuturesTrader,
            this.FuturesDataProvider,
            this.CandlestickMonitor,
            this.Mediator);
        
        this.Candlesticks = this.CandlestickGenerator.Generate(100);
    }
}
