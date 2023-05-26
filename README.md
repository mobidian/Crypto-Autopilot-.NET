# Crypto Autopilot

**Crypto Autopilot** is a cryptocurrency trading software designed to help traders maximize their profits in the cryptocurrency market.  
With advanced built-in technical indicators and real-time market data the application it opens up the possibility to create customizable strategies to execute trades with precision and efficiency.  
Built for traders looking to implement their own strategies using C#, **Crypto Autopilot** offers a developer-friendly solution and the flexibility to program the trade signals in-source or receive them via REST API.  
This powerful solution streamlines the trading process and provides traders with a comprehensive tool for increased chances of success in the dynamic cryptocurrency market.



## Using Crypto Autopilot


### Step 1: Strategy Engine implementation
You will first have to create a class implementing the `IStrategyEngine` interface.  
Inheriting from the `StrategyEngine` abstract class because it already has dependencies on services which make the strategy development much more friendly
and it defines an abstract method `MakeMoveAsync()` which is invoked over and over again until the strategy engine is stopped which makes it the best place to implement out core strategy logic.<br/>

Below is an example of a trading strategy using the [Relative Strength Index (RSI)](https://www.investopedia.com/terms/r/rsi.asp) indicator and the [Exponential Moving Average (EMA)](https://www.investopedia.com/terms/e/ema.asp) to determinte market [entries](https://www.investopedia.com/terms/e/entry-point.asp) and [exits](https://www.investopedia.com/terms/e/exit-point.asp).  
The first thing it does is waiting for the next [candlestick](https://www.investopedia.com/terms/c/candlestick.asp) to be created because it does not need to compute it's core logic any more often than that.  
It enters a [long position](https://www.investopedia.com/terms/l/long.asp) under 2 conditions:  
1) A [bullish](https://www.investopedia.com/terms/b/bull.asp) [RSI divergence](https://www.investopedia.com/terms/r/rsi.asp#toc-example-of-rsi-divergences) is signaled.  
2) The price is above the [EMA](https://www.investopedia.com/terms/e/ema.asp).

It exits a [long position](https://www.investopedia.com/terms/l/long.asp) under 1 condition:
1) A [bearish](https://www.investopedia.com/terms/b/bear.asp) [RSI divergence](https://www.investopedia.com/terms/r/rsi.asp#toc-example-of-rsi-divergences) is signaled.

When a [long position](https://www.investopedia.com/terms/l/long.asp) is opened, a [stop loss](https://www.investopedia.com/terms/s/stop-lossorder.asp) and [take profit](https://www.investopedia.com/terms/t/take-profitorder.asp) orders are placed:  
1) The [stop loss](https://www.investopedia.com/terms/s/stop-lossorder.asp) order is placed at the [EMA](https://www.investopedia.com/terms/e/ema.asp) price.  
2) The [take profit](https://www.investopedia.com/terms/t/take-profitorder.asp) is placed at a higher price taking into account the [stop loss](https://www.investopedia.com/terms/s/stop-lossorder.asp) price and the [Risk Reward Ratio](https://www.investopedia.com/terms/r/riskrewardratio.asp)

The [RSI divergence](https://www.investopedia.com/terms/r/rsi.asp#toc-example-of-rsi-divergences) signal is sent via the `FlagDivergence` public method.  
The comparision between the [EMA](https://www.investopedia.com/terms/e/ema.asp) and the current price is computed in-source, in the `InternalTakeActionAsync` method.  
The [stop loss](https://www.investopedia.com/terms/s/stop-lossorder.asp) and [take profit](https://www.investopedia.com/terms/t/take-profitorder.asp) order prices are also calculated in-source, in the `OpenLongPositionAsync` private method.  

Note: In production your strategy implementation should work with persistent storage for storing the current state of the strategy.

```csharp
public class ExampleStrategyEngine : StrategyEngine
{
    protected readonly CurrencyPair CurrencyPair;
    protected readonly KlineInterval KlineInterval;

    protected readonly int EMALength;

    protected readonly decimal Margin;
    protected readonly decimal RiskRewardRatio;

    public ExampleStrategyEngine(CurrencyPair currencyPair, KlineInterval klineInterval, int emaLength, decimal margin, decimal riskRewardRatio, IBybitUsdFuturesMarketDataProvider marketDataProvider, IBybitUsdPerpetualKlinesMonitor klinesMonitor, IBybitFuturesAccountDataProvider futuresAccount, IBybitUsdFuturesTradingService tradingService) : base(marketDataProvider, klinesMonitor, futuresAccount, tradingService)
    {
        this.CurrencyPair = currencyPair;
        this.KlineInterval = klineInterval;
        this.EMALength = emaLength;
        this.Margin = margin;
        this.RiskRewardRatio = riskRewardRatio;
    }


    internal IList<Candlestick> Candlesticks = default!;
    internal decimal Price;
    internal decimal EMA;
    
    protected override Task TakeActionAsync() => this.InternalTakeActionAsync();
    protected internal async Task InternalTakeActionAsync()
    {
        await this.KlinesMonitor.WaitForNextCandlestickAsync(this.CurrencyPair.Name, this.KlineInterval);


        await this.GetLatestMarketDataAsync();

        var BuyCondition = this.Divergence == RsiDivergence.Bullish && this.Price > this.EMA;
        var SellCondition = this.Divergence == RsiDivergence.Bearish;

        if (BuyCondition && this.TradingService.LongPosition is null)
        {
            await this.OpenLongPositionAsync();
            this.Divergence = null;
        }
        else if (SellCondition && this.TradingService.LongPosition is not null)
        {
            await this.ClosePositionAsync();
            this.Divergence = null;
        }
    }
    private async Task GetLatestMarketDataAsync()
    {
        var bybitKlines = await this.MarketDataProvider.GetCompletedCandlesticksAsync(this.CurrencyPair.Name, this.KlineInterval);
        this.Candlesticks = bybitKlines.Select(x => x.ToCandlestick()).ToList();

        this.Price = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        this.EMA = Convert.ToDecimal(this.Candlesticks.GetEma(this.EMALength).Last().Ema);
    }
    private async Task OpenLongPositionAsync()
    {
        var stopLoss = this.EMA;
        var takeProfit = this.Price + (this.Price - this.EMA) * this.RiskRewardRatio;

        await this.TradingService.OpenPositionAsync(PositionSide.Buy, this.Margin, stopLoss, takeProfit);
        // // maybe publish a notification // //
    }
    private async Task ClosePositionAsync()
    {
        await this.TradingService.ClosePositionAsync(PositionSide.Buy);
        // // maybe publish a notification // //
    }

    
    /// <summary>
    /// <para>Gets the current Divergence that has occured in the market.</para>
    /// <para>A null value indicates that there is no divergence or it has been consumed.</para>
    /// </summary>
    public RsiDivergence? Divergence { get; private set; }

    /// <summary>
    /// Informs the engine about a divergence that has occured in the market
    /// </summary>
    /// <param name="divergence">The divergence that has occured in the market</param>
    public void FlagDivergence(RsiDivergence divergence) => this.Divergence = divergence;
}
```

One of the key factors in developing and maintaining a successful trading strategy is unsurprisingly the ability to unit-test it.
To ensure your trading strategies are unit-testable, the class should have an internal method that can execute the core logic of the strategy, such as `InternalTakeActionAsync()`,
which can be invoked during the unit test, the fields containing the current state of the strategy should be internal as well so that they can be verified during the unit test and the internals visible in the test project.


### Step 2: Strategy setup isolation
Create a class that implements the `IStrategyEndpoints<YourStrategy>` interface.  
In the `AddStrategy()` method, register your strategy in the DI container as a singleton service.  
In the `MapStrategySignalsEndpoints()` method, map the endpoints associated with the strategy if there are any.

```csharp
public class ExampleStrategyEndpoints : IStrategyEndpoints<ExampleStrategyEngine>
{
    public static void AddStrategy(IServiceCollection services, IConfiguration configuration)
    {
        var currencyPair = new CurrencyPair("ETH", "BUSD");
        var klineInterval = KlineInterval.OneMinute;
        var emaLength = 50;
        var margin = 20m;
        var riskRewardRatio = 3;
        var leverage = 10;
        
        services.AddSingleton<ExampleStrategyEngine>(services =>
            new ExampleStrategyEngine(
               currencyPair,
               klineInterval,
               emaLength,
               margin,
               riskRewardRatio,
               services.GetRequiredService<IBybitUsdFuturesMarketDataProvider>(),
               services.GetRequiredService<IBybitUsdPerpetualKlinesMonitor>(),
               services.GetRequiredService<IBybitFuturesAccountDataProvider>(),
               services.GetRequiredService<BybitUsdFuturesTradingServiceFactory>().Create(currencyPair, leverage, services)));
    }

    public static void MapStrategySignalsEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("ExampleStrategy", ([FromServices] ExampleStrategyEngine engine, [FromQuery] RsiDivergence divergence) => engine.FlagDivergence(divergence)).WithTags(nameof(ExampleStrategyEngine));
    }
}
```

### Step 3: Stategy registration and endpoint mapping

You will now need to register the services needed for the strategies to be executed as well as the strategies themselves.  

Call the `AddServices()` extension method on the `IServiceCollection` to register the services needed to be able to run your strategy.  

Call the `AddStrategies<TMarker>()` extension method on the `IServiceCollection` to register your strategy endpoints in the DI container.
This will dynamically invoke the `AddStrategy()` method on all classes implementing the `IStrategyEndpoints<TMarker>` interface from the assembly containing the `TMarker` type.

Call the `MapStrategyEndpoints<TMarker>()` extension method on the `IApplicationBuilder` to map your strategy endpoints.
This will dynamically invoke the `MapStartStopEndpoints()` and `MapStrategySignalsEndpoints()` methods on all classes implementing the `IStrategyEndpoints<TMarker>` interface from the assembly containing the `TMarker` type.

```csharp
builder.Services.AddServices(builder.Configuration);
builder.Services.AddStrategies<Program>(builder.Configuration);

var app = builder.Build();

app.MapStrategyEndpoints<Program>();
```

### Step 4: Running the API
You are now all set.  
Configure the alerts from the charting platform of your choice, unless you've done so already, and let **Crypto Autopilot** do the heavy lifting.

<br/>

##### DISCLAIMER:
**The strategies and information provided in this project are intended for educational purposes only and are not to be considered investment advice. Trading cryptocurrencies carries a high level of risk and the market can be highly volatile, especially when using leverage.**  
**All investment decisions made using the information provided in this project are made solely at your own risk.**  
**Crypto Autopilot, its developers, and any associated entities are not liable for any financial losses that may occur as a result of using this project or following the provided trading strategies.**