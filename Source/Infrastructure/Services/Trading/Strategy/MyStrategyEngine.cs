using Application.Interfaces.Services.Trading;
using Application.Interfaces.Services.Trading.Strategy;

using Binance.Net.Enums;

using Domain.Models;

namespace Infrastructure.Services.Trading.Strategy;

public class MyStrategyEngine : IStrategyEngine
{
    public CurrencyPair CurrencyPair { get; }
    public KlineInterval KlineInterval { get; }

    private readonly ICfdTradingService FuturesTrader;
    private readonly ICfdMarketDataProvider FuturesDataProvider;

    public MyStrategyEngine()
    {

    }

    //// //// ////

    private volatile bool ShouldContinue = true;
    private volatile bool Stopped = false;

    public async Task StartTradingAsync()
    {
        while (this.ShouldContinue)
        {
            await this.WaitForNextCandleAsync();
            // get the last open price (if needed)


            await this.MakeMoveAsync();
        }

        this.Stopped = true;
    }

    private async Task MakeMoveAsync()
    {
        var candlesticks = await this.FuturesDataProvider.GetCompletedCandlesticksAsync(this.KlineInterval);

        // // TODO // //
    }

    private async Task WaitForNextCandleAsync()
    {
        // // TODO // //
        // implement using a service that is subscribed to usd futures streams
    }


    public async Task StopTradingAsync()
    {
        this.ShouldContinue = false;
        
        while (!this.Stopped)
        {
            await Task.Delay(20);
        }
    }

    //// //// ////    

    public void Dispose()
    {
        this.StopTradingAsync().GetAwaiter().GetResult();
        this.DisposeProperties();
    }
    public async ValueTask DisposeAsync()
    {
        await this.StopTradingAsync();
        this.DisposeProperties();
    }
    
    private void DisposeProperties()
    {
        try
        {
            this.FuturesTrader.Dispose();
            this.FuturesDataProvider.Dispose();
        }
        finally { GC.SuppressFinalize(this); }
    }
}
