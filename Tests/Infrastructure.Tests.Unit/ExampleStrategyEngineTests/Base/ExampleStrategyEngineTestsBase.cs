using Application.Interfaces.Services.Trading;
using Application.Interfaces.Services.Trading.Monitors;

using Binance.Net.Enums;

using Domain.Models;

using Generated;

using Infrastructure.Strategies.Example;

using MediatR;

using Skender.Stock.Indicators;

namespace Infrastructure.Tests.Unit.ExampleStrategyEngineTests.Base;

public class ExampleStrategyEngineTestsBase
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

    protected IReadOnlyList<Candlestick> RandomCandlesticks = default!;
    
    protected ExampleStrategyEngine SUT = default!;
    protected readonly int EMALength;
    protected readonly decimal Margin;
    protected readonly decimal RiskRewardRatio;

    protected readonly CurrencyPair CurrencyPair = new CurrencyPair("ETH", "BUSD");
    protected readonly KlineInterval KlineInterval = KlineInterval.FifteenMinutes;

    protected readonly ICfdTradingService FuturesTrader = Substitute.For<ICfdTradingService>();
    protected readonly ICfdMarketDataProvider FuturesDataProvider = Substitute.For<ICfdMarketDataProvider>();
    protected readonly IFuturesCandlesticksMonitor CandlestickMonitor = Substitute.For<IFuturesCandlesticksMonitor>();
    protected readonly IMediator Mediator = Substitute.For<IMediator>();

    protected readonly IIndicatorsAdapter IndicatorsAdapter = Substitute.For<IIndicatorsAdapter>();

    public ExampleStrategyEngineTestsBase()
    {
        this.EMALength = this.Random.Next(10, 100);
        this.Margin = this.Random.Next(100, 1000);
        this.RiskRewardRatio = Convert.ToDecimal(1 + this.Random.Next(1, 20) * this.Random.NextDouble());

        this.SUT = new ExampleStrategyEngine(this.CurrencyPair, this.KlineInterval, this.EMALength, this.Margin, this.RiskRewardRatio, this.FuturesTrader, this.FuturesDataProvider, this.CandlestickMonitor, this.Mediator)
        {
            IndicatorsAdapter = this.IndicatorsAdapter,
        };

        this.RandomCandlesticks = this.CandlestickGenerator.Generate(100);
    }

    
    //// //// ARRANGES //// ////
    
    // The arranges are kept here because each arrange is needed in multiple tests
    
    protected void ArrangeFor_OuterSignalBuy_ShouldTriggerPositionOpening_WhenPriceIsAboveEmaAndTraderIsNotInPosition(out decimal currentPrice, out decimal emaPrice)
    {
        currentPrice = this.Random.Next(1000, 3000);
        emaPrice = currentPrice - this.Random.Next(1, 100); // EMA > PRICE

        this.FuturesDataProvider.GetCurrentPriceAsync(Arg.Is(this.CurrencyPair.Name)).Returns(currentPrice);
        this.FuturesTrader.IsInPosition().Returns(false, true); // trader not in position
        this.FuturesDataProvider.GetCompletedCandlesticksAsync(this.CurrencyPair.Name, this.KlineInterval).Returns(this.RandomCandlesticks);
        this.IndicatorsAdapter.GetEma(this.EMALength).Returns(new EmaResult[] { new EmaResult(DateTime.MinValue) { Ema = Convert.ToDouble(emaPrice) } });
    }
    protected void ArrangeFor_OuterSignalBuy_ShouldNotTriggerPositionOpening_WhenTraderIsAlreadyInPosition(out decimal currentPrice, out decimal emaPrice)
    {
        currentPrice = this.Random.Next(1000, 3000);
        emaPrice = currentPrice - this.Random.Next(10, 50); ; // EMA > PRICE

        this.FuturesDataProvider.GetCurrentPriceAsync(Arg.Is(this.CurrencyPair.Name)).Returns(currentPrice);
        this.FuturesTrader.IsInPosition().Returns(true); // trader in position
        this.FuturesDataProvider.GetCompletedCandlesticksAsync(this.CurrencyPair.Name, this.KlineInterval).Returns(this.RandomCandlesticks);
        this.IndicatorsAdapter.GetEma(this.EMALength).Returns(new EmaResult[] { new EmaResult(DateTime.MinValue) { Ema = Convert.ToDouble(emaPrice) } });
    }
    protected void ArrangeFor_OuterSignalBuy_ShouldNotTriggerPositionOpening_WhenPriceIsNotAboveEma(out decimal currentPrice, out decimal emaPrice)
    {
        currentPrice = this.Random.Next(1000, 3000);
        emaPrice = currentPrice + this.Random.Next(0, 50); // EMA < PRICE
        
        this.FuturesDataProvider.GetCurrentPriceAsync(Arg.Is(this.CurrencyPair.Name)).Returns(currentPrice);
        this.FuturesTrader.IsInPosition().Returns(false, true); // trader not in position
        this.FuturesDataProvider.GetCompletedCandlesticksAsync(this.CurrencyPair.Name, this.KlineInterval).Returns(this.RandomCandlesticks);
        this.IndicatorsAdapter.GetEma(this.EMALength).Returns(new EmaResult[] { new EmaResult(DateTime.MinValue) { Ema = Convert.ToDouble(emaPrice) } });
    }
    
    protected void ArrangeFor_OuterSignalSell_ShouldTriggerPositionClosing_WhenTraderIsInPosition(out decimal currentPrice)
    {
        currentPrice = this.Random.Next(1000, 3000);
        this.FuturesDataProvider.GetCurrentPriceAsync(Arg.Is(this.CurrencyPair.Name)).Returns(currentPrice);
        this.FuturesTrader.IsInPosition().Returns(true); // trader in position
        this.FuturesDataProvider.GetCompletedCandlesticksAsync(Arg.Any<string>(), Arg.Any<KlineInterval>()).Returns(this.RandomCandlesticks);
        this.IndicatorsAdapter.GetEma(this.EMALength).Returns(new EmaResult[] { new EmaResult(DateTime.MinValue) { Ema = -1.0 } });
    }
    protected void ArrangeFor_OuterSignalSell_ShouldNotTriggerPositionClosing_WhenTraderIsInNotPosition()
    {
        this.FuturesTrader.IsInPosition().Returns(false, true); // trader not in position
        this.IndicatorsAdapter.GetEma(this.EMALength).Returns(new EmaResult[] { new EmaResult(DateTime.MinValue) { Ema = -1.0 } });
    }
}
