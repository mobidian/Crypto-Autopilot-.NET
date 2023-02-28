# Crypto Autopilot .NET

**Crypto Autopilot** is a cryptocurrency trading software designed to help traders maximize their profits in the cryptocurrency market.
With advanced built-in technical indicators and real-time market data the application opens up the possibility to create customizable strategies to execute trades with precision and efficiency.
<br>Built for traders looking to implement their own strategies using C#, Crypto Autopilot offers a developer-friendly solution and the flexibility to program the trade signals in-source or receive them via REST API. 
This powerful solution streamlines the trading process and provides traders with a comprehensive tool for increased chances of success in the dynamic cryptocurrency market.<br>



## Using Crypto Autopilot


### Step 1: Strategy Engine implementation
#### Create a non-abstract class implementing the IStrategyEngine interface. <br>It is recommended to inherit from the StrategyEngine abstract class as it provides some generic functionality. The core strategy logic should be implemented in the MakeMoveAsync() method.<br>

```csharp
public class ExampleStrategyEngine : StrategyEngine
{
    protected readonly int EMALength;

    protected readonly decimal Margin;
    protected readonly decimal RiskRewardRatio;

    internal ExampleStrategyEngine(Guid guid, CurrencyPair currencyPair, KlineInterval klineInterval) : base(guid, currencyPair, klineInterval) { }
    public ExampleStrategyEngine(CurrencyPair currencyPair, KlineInterval klineInterval, int emaLength, decimal margin, decimal riskRewardRatio, ICfdTradingService futuresTrader, ICfdMarketDataProvider futuresDataProvider, IFuturesCandlesticksMonitor candlestickMonitor, IMediator mediator) : base(currencyPair, klineInterval, futuresTrader, futuresDataProvider, candlestickMonitor, mediator)
    {
        this.EMALength = emaLength;
        this.Margin = margin;
        this.RiskRewardRatio = riskRewardRatio;
        this.IndicatorsAdapter = new IndicatorsAdapter(this.Candlesticks);
    }
    
    private IList<Candlestick> Candlesticks = default!;
    internal IIndicatorsAdapter IndicatorsAdapter = default!;
    internal decimal Price;
    internal decimal EMA;
    
    internal override async Task MakeMoveAsync()
    {
        await this.CandlestickMonitor.WaitForNextCandlestickAsync(this.CurrencyPair.Name, ContractType.Perpetual, this.KlineInterval);


        await GetLatestMarketDataAsync();

        var BuyCondition = this.Divergence == RsiDivergence.Bullish && this.Price > this.EMA;
        var SellCondition = this.Divergence == RsiDivergence.Bearish;
        
        if (BuyCondition && !this.FuturesTrader.IsInPosition())
        {
            await this.OpenLongPositionAsync();
            this.Divergence = null;
        }
        else if (SellCondition && this.FuturesTrader.IsInPosition())
        {
            await this.ClosePositionAsync();
            this.Divergence = null;
        }
    }
    private async Task GetLatestMarketDataAsync()
    {
        this.Candlesticks = (await this.FuturesDataProvider.GetCompletedCandlesticksAsync(this.CurrencyPair.Name, this.KlineInterval)).ToList();
        this.Price = await this.FuturesTrader.GetCurrentPriceAsync();
        this.EMA = Convert.ToDecimal(this.IndicatorsAdapter.GetEma(this.EMALength).Last().Ema);
    }
    private async Task OpenLongPositionAsync()
    {
        decimal stopLoss = this.EMA;
        decimal takeProfit = this.Price + (this.Price - this.EMA) * this.RiskRewardRatio;

        await this.FuturesTrader.OpenPositionAtMarketPriceAsync(OrderSide.Buy, this.Margin, stopLoss, takeProfit);
        await this.Mediator.Publish(new PositionOpenedNotification(this.Candlesticks.Last(), this.FuturesTrader.Position!));
    }
    private async Task ClosePositionAsync()
    {
        var closingOrder = await this.FuturesTrader.ClosePositionAsync();
        await this.Mediator.Publish(new PositionClosedNotification(this.Candlesticks.Last(), closingOrder));
    }
    
    
    // a null value indicates that there is no divergence or it has been consumed
    public RsiDivergence? Divergence { get; private set; }
    
    /// <summary>
    /// Informs the engine about a divergence that has occured in the market
    /// </summary>
    /// <param name="divergence">The divergence that has occured in the market</param>
    public void FlagDivergence(RsiDivergence divergence) => this.Divergence = divergence;
}
```

#### This is an example strategy for entering long positions. The strategy can be alerted about RSI divergences and will enter a long position if a bullish divergence is flagged and the price is above the exponential moving average (EMA).<br>It utilizes both outer alerts (RSI divergences) and in source signals (Price > EMA) to make its decisions.<br>It is recommended to keep the properties such as IIndicatorsAdapter and those storing values of technical indicators internal and visible in the test projects, to keep the class unit testable.<br>


### Step 2: Singleton Registration and endpoint mapping
#### Create a class that implements the IStrategyEndpoints<YourStrategy> interface. Register your trading strategy in the ASP.NET Core Web API service collection in the 'AddServices' method. Map the endpoints associated with the strategy signals using the 'MapStrategySignalsEndpoints' method. The endpoints for starting and stopping the strategy will be automatically mapped.

```csharp
public class ExampleStrategyEndpoints : IStrategyEndpoints<ExampleStrategyEngine>
{
    public static void AddStrategy(IServiceCollection services, IConfiguration configuration)
    {
        var currencyPair = new CurrencyPair("ETH", "BUSD");
        var timeframe = KlineInterval.OneMinute;
        var emaLength = 50;
        var margin = 20m;
        var riskRewardRatio = 3;
        var leverage = 10;
        
        services.AddSingleton<ExampleStrategyEngine>(services => 
            new ExampleStrategyEngine(
               currencyPair,
               timeframe,
               emaLength,
               margin,
               riskRewardRatio,
               services.GetRequiredService<ICfdTradingServiceFactory>().Create(currencyPair, leverage, services),
               services.GetRequiredService<ICfdMarketDataProvider>(),
               services.GetRequiredService<IFuturesMarketsCandlestickAwaiterFactory>().Create(currencyPair, timeframe, services),
               services.GetRequiredService<IMediator>()));
    }
    
    public static void MapStrategySignalsEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("ExampleStrategy", ([FromServices] ExampleStrategyEngine engine, [FromQuery] RsiDivergence divergence) => engine.FlagDivergence(divergence)).WithTags(nameof(ExampleStrategyEngine));
    }
}
```


### Step 3: Running the API
#### You are now all set.<br>Configure the alerts from the charting platform of your choice and let Crypto Autopilot do the heavy lifting.<br>



##### WARNING: The strategies and information provided in this project are intended for educational purposes only and are not to be considered investment advice. Trading cryptocurrencies carries a high level of risk and the market can be highly volatile, especially when using leverage.<br>All investment decisions made using the information provided in this project are made solely at your own risk.<br>Crypto Autopilot, its developers, and any associated entities are not liable for any financial losses that may occur as a result of using this project or following the provided trading strategies.<br>
