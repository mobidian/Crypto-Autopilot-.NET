using Application.Exceptions;
using Application.Interfaces.Services.Trading;

using Binance.Net.Enums;
using Binance.Net.Interfaces.Clients.UsdFuturesApi;
using Binance.Net.Objects.Models.Futures;

using CryptoExchange.Net.Objects;

using Domain.Extensions;
using Domain.Models;

using Infrastructure.Extensions;

using Polly;

namespace Infrastructure.Services.Trading;

public class BinanceFuturesTradingService : IFuturesTradingService
{
    public CurrencyPair CurrencyPair { get; }
    public decimal Leverage { get; }

    private readonly IBinanceFuturesApiService FuturesApiService;
    private readonly IBinanceClientUsdFuturesApiTrading TradingClient; // TODO replace with IBinanceFuturesApiService //
    private readonly IBinanceFuturesAccountDataProvider AccountDataProvider;
    private readonly IFuturesMarketDataProvider MarketDataProvider;
    
    public FuturesPosition? Position { get; private set; }

    private readonly int NrDecimals = 2;
    
    public BinanceFuturesTradingService(CurrencyPair currencyPair, decimal leverage, IBinanceFuturesApiService futuresApiService, IBinanceClientUsdFuturesApiTrading tradingClient, IBinanceFuturesAccountDataProvider accountDataProvider, IFuturesMarketDataProvider marketDataProvider)
    {
        this.CurrencyPair = currencyPair ?? throw new ArgumentNullException(nameof(currencyPair));
        this.Leverage = leverage;
        
        this.FuturesApiService = futuresApiService ?? throw new ArgumentNullException(nameof(futuresApiService));
        this.TradingClient = tradingClient ?? throw new ArgumentNullException(nameof(tradingClient));
        this.AccountDataProvider = accountDataProvider ?? throw new ArgumentNullException(nameof(accountDataProvider));
        this.MarketDataProvider = marketDataProvider ?? throw new ArgumentNullException(nameof(marketDataProvider));
    }

    //// //// ////

    private readonly IAsyncPolicy<WebCallResult<BinanceFuturesPlacedOrder>> SlTpRetryPolicy =
        Policy<WebCallResult<BinanceFuturesPlacedOrder>>
            .Handle<InternalTradingServiceException>(exc =>
            {
                // These if statements check for specific error messages related to user input errors
                // Errors raised as a result of bad user input won't be handled

                if (exc.Message.EndsWith("Error: -1102: Mandatory parameter 'stopPrice' was not sent, was empty/null, or malformed."))
                    return false;

                if (exc.Message.EndsWith("Error: -2021: Order would immediately trigger."))
                    return false;

                return true;
            })
            .WaitAndRetryAsync(3, retryCount => TimeSpan.FromSeconds(Math.Round(Math.Pow(1.6, retryCount), 2))); // 1.6 sec, 2.56 sec, 4.1 sec
    

    public async Task<IEnumerable<BinanceFuturesOrder>> PlaceMarketOrderAsync(OrderSide OrderSide, decimal QuoteMargin = decimal.MaxValue, decimal? StopLoss = null, decimal? TakeProfit = null)
    {
        if (this.IsInPosition())
        {
            throw new InvalidOperationException("A position is open already");
        }
        

        var orders = await this.FuturesApiService.PlaceMarketOrderAsync(this.CurrencyPair.Name, OrderSide, QuoteMargin, this.Leverage, StopLoss, TakeProfit);
        var ordersArray = orders.ToArray();
        
        this.Position = new FuturesPosition
        {
            CurrencyPair = this.CurrencyPair,

            Leverage = this.Leverage,
            Margin = QuoteMargin,

            EntryOrder = ordersArray[0],
            StopLossOrder = ordersArray[1].Id != 0 ? ordersArray[1] : null,
            TakeProfitOrder = ordersArray[2].Id != 0 ? ordersArray[2] : null
        };
        
        return ordersArray;
    }
    
    public async Task<BinanceFuturesOrder> PlaceLimitOrderAsync(OrderSide OrderSide, decimal LimitPrice, decimal QuoteMargin = decimal.MaxValue, decimal? StopLoss = null, decimal? TakeProfit = null)
    {
        if (this.IsInPosition())
        {
            throw new InvalidOperationException("A position is open already");
        }
        
        
        return await this.FuturesApiService.PlaceLimitOrderAsync(this.CurrencyPair.Name, OrderSide, LimitPrice, QuoteMargin, this.Leverage, StopLoss, TakeProfit);
    }

    public async Task<BinanceFuturesPlacedOrder> PlaceStopLossAsync(decimal price)
    {
        if (this.Position is null)
        {
            throw new InvalidOperationException("No position is open thus a stop loss can't be placed");
        }


        var SLPlacingCallResult = await this.SlTpRetryPolicy.ExecuteAsync(async () =>
        {
            var SLPlacingCallResult = await this.TradingClient.PlaceOrderAsync(symbol: this.CurrencyPair.Name, side: this.Position.EntryOrder.Side.Invert(), type: FuturesOrderType.StopMarket, quantity: this.Position.EntryOrder.Quantity, stopPrice: Math.Round(price, this.NrDecimals), positionSide: this.Position.Side);
            SLPlacingCallResult.ThrowIfHasError("The stop loss could not be placed");
            return SLPlacingCallResult;
        });
        
        var GetSLOrderTask = this.MarketDataProvider.GetOrderAsync(this.CurrencyPair.Name, SLPlacingCallResult!.Data.Id);
        if (this.Position.StopLossOrder is not null)
        {
            await this.FuturesApiService.CancelOrderAsync(this.CurrencyPair.Name, this.Position.StopLossOrder.Id);
        }

        this.Position.StopLossOrder = await GetSLOrderTask;
        
        return SLPlacingCallResult.Data;
    }

    public async Task<BinanceFuturesPlacedOrder> PlaceTakeProfitAsync(decimal price)
    {
        if (this.Position is null)
        {
            throw new InvalidOperationException("No position is open thus a take profit can't be placed");
        }

        
        var TPPlacingCallResult = await this.SlTpRetryPolicy.ExecuteAsync(async () =>
        {
            var TPPlacingCallResult = await this.TradingClient.PlaceOrderAsync(symbol: this.CurrencyPair.Name, side: this.Position.EntryOrder.Side.Invert(), type: FuturesOrderType.TakeProfitMarket, quantity: this.Position.EntryOrder.Quantity, stopPrice: Math.Round(price, this.NrDecimals), positionSide: this.Position.Side);
            TPPlacingCallResult.ThrowIfHasError("The take profit could not be placed");
            return TPPlacingCallResult;
        });
        
        var GetTPOrderTask = this.MarketDataProvider.GetOrderAsync(this.CurrencyPair.Name, TPPlacingCallResult!.Data.Id);
        if (this.Position.TakeProfitOrder is not null)
        {
            await this.FuturesApiService.CancelOrderAsync(this.CurrencyPair.Name, this.Position.TakeProfitOrder.Id);
        }

        this.Position.TakeProfitOrder = await GetTPOrderTask;

        return TPPlacingCallResult.Data;
    }

    public async Task<BinanceFuturesOrder> ClosePositionAsync()
    {
        if (this.Position is null)
        {
            throw new InvalidOperationException($"No position is open", new NullReferenceException($"{nameof(this.Position)} is NULL"));
        }


        var ClosingCallResult = await this.TradingClient.PlaceOrderAsync(symbol: this.CurrencyPair.Name, side: this.Position.EntryOrder.Side.Invert(), type: FuturesOrderType.Market, quantity: this.Position.EntryOrder.Quantity, positionSide: this.Position.Side);
        ClosingCallResult.ThrowIfHasError("The current position could not be closed");
        
        var GetFuturesClosingOrderTask = this.MarketDataProvider.GetOrderAsync(this.CurrencyPair.Name, ClosingCallResult.Data.Id);
        
        // some orders may still be open so they must be closed
        await this.FuturesApiService.CancelOrdersAsync(this.CurrencyPair.Name, this.Position.GetOpenOrdersIDs().ToList());

        this.Position = null;
        return await GetFuturesClosingOrderTask;
    }

    public bool IsInPosition() => this.Position is not null;
    

    //// //// ////


    private bool Disposed = false;

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (this.Disposed)
            return;

        if (disposing)
            this.MarketDataProvider.Dispose();

        this.Disposed = true;
    }
}
