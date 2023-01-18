using System.Text;

using Application.Exceptions;
using Application.Interfaces.Services.Trading;

using Binance.Net.Clients;
using Binance.Net.Clients.UsdFuturesApi;
using Binance.Net.Enums;
using Binance.Net.Interfaces.Clients;
using Binance.Net.Interfaces.Clients.UsdFuturesApi;
using Binance.Net.Objects.Models.Futures;
using Binance.Net.Objects.Models.Futures.AlgoOrders;

using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;

using Domain.Extensions;
using Domain.Models;

using Infrastructure.Common;

namespace Infrastructure.Services.Trading;

public class BinanceCfdTradingService : ICfdTradingService
{
    public CurrencyPair CurrencyPair { get; }
    public decimal Leverage { get; }

    private readonly IBinanceClient BinanceClient;
    private readonly IBinanceClientUsdFuturesApi FuturesClient;
    private readonly IBinanceClientUsdFuturesApiTrading TradingClient;
    private readonly IBinanceClientUsdFuturesApiExchangeData ExchangeData;
    
    public FuturesPosition? Position { get; private set; }

    private readonly int NrDecimals = 2;

    public BinanceCfdTradingService(CurrencyPair CurrencyPair, ApiCredentials ApiCredentials, decimal Leverage = 10)
    {
        this.CurrencyPair = CurrencyPair ?? throw new ArgumentNullException(nameof(CurrencyPair));

        this.BinanceClient = new BinanceClient();
        this.BinanceClient.SetApiCredentials(ApiCredentials ?? throw new ArgumentNullException(nameof(ApiCredentials)));
        this.FuturesClient = this.BinanceClient.UsdFuturesApi;
        this.TradingClient = this.BinanceClient.UsdFuturesApi.Trading;
        this.ExchangeData = this.FuturesClient.ExchangeData;
        
        this.Leverage = Leverage;
    }

    //// //// ////

    public async Task<BinanceFuturesOrder> GetOrderAsync(long orderID)
    {
        var callResult = await this.TradingClient.GetOrderAsync(symbol: this.CurrencyPair.Name, orderId: orderID);
        callResult.ThrowIfHasError();

        return callResult.Data;
    }

    /////  /////

    public async Task<decimal> GetCurrentPriceAsync()
    {
        var callResult = await this.ExchangeData.GetPriceAsync(this.CurrencyPair.Name);
        callResult.ThrowIfHasError();

        return callResult.Data.Price;
    }
    public async Task<decimal> GetEquityAsync()
    {
        var callResult = await this.FuturesClient.Account.GetAccountInfoAsync();
        callResult.ThrowIfHasError("Could not get the account information");

        var asset = callResult.Data.Assets.Where(binanceAsset => this.CurrencyPair.Name.EndsWith(binanceAsset.Asset)).Single();
        return asset.AvailableBalance;
    }


    private async Task<(decimal BaseQuantity, decimal CurrentPrice)> Get_BaseQuantity_and_CurrentPrice_Async(decimal MarginBUSD)
    {
        decimal BaseQuantity;
        decimal equityBUSD;
        decimal CurrentPrice;
        
        var GetCurrentPrice_Task = this.GetCurrentPriceAsync();
        equityBUSD = MarginBUSD == decimal.MaxValue ? await this.GetEquityAsync() : MarginBUSD;
        CurrentPrice = await GetCurrentPrice_Task;

        BaseQuantity = Math.Round(equityBUSD * this.Leverage / CurrentPrice, this.NrDecimals, MidpointRounding.ToZero);

        return (BaseQuantity, CurrentPrice);
    }
    private static void ValidateInputAgainstCurrentPrice(OrderSide OrderSide, decimal? StopLoss_price, decimal? TakeProfit_price, decimal CurrentPrice)
    {
        StringBuilder builder = new StringBuilder();

        #region SL/TP validate positive
        if (StopLoss_price <= 0)
            builder.AppendLine($"Specified {nameof(StopLoss_price)} was negative");

        if (TakeProfit_price <= 0)
            builder.AppendLine($"Specified {nameof(TakeProfit_price)} was negative");

        if (builder.Length != 0)
            throw new InvalidOrderException(builder.Remove(builder.Length - 1, 1).ToString());
        #endregion
        
        #region SL/TP validate against current price
        if (OrderSide == OrderSide.Buy)
        {
            if (StopLoss_price >= CurrentPrice)
                builder.AppendLine($"The stop loss can't be greater than or equal to the current price for a {OrderSide} order, current price was {CurrentPrice} and stop loss was {StopLoss_price}");

            if (TakeProfit_price <= CurrentPrice)
                builder.AppendLine($"The take profit can't be less greater than or equal to the current price for a {OrderSide} order, current price was {CurrentPrice} and take profit was {TakeProfit_price}");
        }
        else
        {
            if (StopLoss_price <= CurrentPrice)
                builder.AppendLine($"The stop loss can't be less greater than or equal to the current price for a {OrderSide} order, current price was {CurrentPrice} and stop loss was {StopLoss_price}");

            if (TakeProfit_price >= CurrentPrice)
                builder.AppendLine($"The take profit can't be greater than or equal to the current price for a {OrderSide} order, current price was {CurrentPrice} and take profit was {TakeProfit_price}");
        }
        
        if (builder.Length != 0)
            throw new InvalidOrderException(builder.Remove(builder.Length - 1, 1).ToString());
        #endregion
    }
    private List<BinanceFuturesBatchOrder> CreateBinanceBatchOrders(OrderSide OrderSide, decimal? StopLoss_price, decimal? TakeProfit_price, decimal BaseQuantity)
    {
        List<BinanceFuturesBatchOrder> BatchOrders = new()
        {
            new BinanceFuturesBatchOrder
            {
                Symbol = this.CurrencyPair.Name,
                Side = OrderSide,
                Type = FuturesOrderType.Market,
                Quantity = BaseQuantity,
            }
        };
        if (StopLoss_price.HasValue)
        {
            BatchOrders.Add(new BinanceFuturesBatchOrder
            {
                Symbol = this.CurrencyPair.Name,
                Side = OrderSide.Invert(),
                Type = FuturesOrderType.StopMarket,
                Quantity = BaseQuantity,
                StopPrice = Math.Round(StopLoss_price.Value, this.NrDecimals),
            });
        }
        if (TakeProfit_price.HasValue)
        {
            BatchOrders.Add(new BinanceFuturesBatchOrder
            {
                Symbol = this.CurrencyPair.Name,
                Side = OrderSide.Invert(),
                Type = FuturesOrderType.TakeProfitMarket,
                Quantity = BaseQuantity,
                StopPrice = Math.Round(TakeProfit_price.Value, this.NrDecimals),
            });
        }

        return BatchOrders;
    }
    private FuturesPosition CreateFuturesPosition(CallResult<IEnumerable<CallResult<BinanceFuturesPlacedOrder>>> PlacedOrdersCallResult, decimal MarginBUSD)
    {
        var PlacedOrders = PlacedOrdersCallResult.Data.Select(call => call.Data).Where(placedOrder => placedOrder is not null).ToList();
        var FuturesOrders = Enumerable.Range(0, 3).Select(_ => new BinanceFuturesOrder()).ToList();
        Parallel.For(0, PlacedOrders.Count, i => FuturesOrders[i] = this.GetOrderAsync(PlacedOrders[i].Id).GetAwaiter().GetResult() ?? new BinanceFuturesOrder()); // Parallel.For isn't async await friendly

        // RARE EXCEPTION: ArgumentException : The EntryOrder property was given a StopMarket order instead of a market order (Parameter 'value')
        // TODO retry policy //
        // Error happened due to binance exception
        // Discovered in the 12th run of a "run ultil failure integration testing

        return new FuturesPosition
        {
            CurrencyPair = this.CurrencyPair,

            Leverage = this.Leverage,
            Margin = MarginBUSD,

            EntryOrder = FuturesOrders[0],
            StopLossOrder = FuturesOrders[1].Id != 0 ? FuturesOrders[1] : null,
            TakeProfitOrder = FuturesOrders[2].Id != 0 ? FuturesOrders[2] : null
        };
    }
    /// <summary>
    /// Opens a new position
    /// </summary>
    /// <param name="OrderSide"></param>
    /// <param name="MarginBUSD"></param>
    /// <param name="StopLoss_price"></param>
    /// <param name="TakeProfit_price"></param>
    /// <returns>The orders that were placed to open the position</returns>
    /// <exception cref="InvalidOrderException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="InternalTradingServiceException"></exception>
    public async Task<IEnumerable<BinanceFuturesPlacedOrder>> OpenPositionAtMarketPriceAsync(OrderSide OrderSide, decimal MarginBUSD = decimal.MaxValue, decimal? StopLoss_price = null, decimal? TakeProfit_price = null)
    {
        if (this.IsInPosition())
        {
            throw new InvalidOperationException("A position is open already");
        }
        

        (decimal BaseQuantity, decimal CurrentPrice) = await this.Get_BaseQuantity_and_CurrentPrice_Async(MarginBUSD);
        ValidateInputAgainstCurrentPrice(OrderSide, StopLoss_price, TakeProfit_price, CurrentPrice);

        var BatchOrders = this.CreateBinanceBatchOrders(OrderSide, StopLoss_price, TakeProfit_price, BaseQuantity);
        var PlaceOrdersCallResult = await this.TradingClient.PlaceMultipleOrdersAsync(orders: BatchOrders.ToArray());
        PlaceOrdersCallResult.ThrowIfHasError();


        this.Position = this.CreateFuturesPosition(PlaceOrdersCallResult, MarginBUSD);
        
        return PlaceOrdersCallResult.Data.Select(callRes => callRes.Data);
    }

    /// <summary>
    /// Closes the existing position
    /// </summary>
    /// <returns>The futures order that was placed to close the position</returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InternalTradingServiceException"></exception>
    public async Task<BinanceFuturesOrder> ClosePositionAsync()
    {
        if (this.Position is null)
        {
            throw new InvalidOperationException($"No position is open", new NullReferenceException($"{nameof(this.Position)} is NULL"));
        }


        var ClosingCallResult = await this.TradingClient.PlaceOrderAsync(symbol: this.CurrencyPair.Name, side: this.Position.EntryOrder.Side.Invert(), type: FuturesOrderType.Market, quantity: this.Position.EntryOrder.Quantity);
        ClosingCallResult.ThrowIfHasError("The current position could not be closed");

        var GetFuturesClosingOrderTask = this.GetOrderAsync(ClosingCallResult.Data.Id);
        
        // some orders may still be open so they must be closed
        var CancellingCallResult = await this.TradingClient.CancelMultipleOrdersAsync(symbol: this.CurrencyPair.Name, this.Position.GetOpenOrdersIDs().ToList());
        CancellingCallResult.ThrowIfHasError("The remaining orders could not be closed");

        this.Position = null;
        return await GetFuturesClosingOrderTask;
    }

    /// <summary>
    /// Places a stop loss futures order and then updates this.Position.StopLossOrder
    /// </summary>
    /// <param name="price"></param>
    /// <returns>The order that has been placed as a <see cref="BinanceFuturesPlacedOrder"/></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="InternalTradingServiceException"></exception>
    public async Task<BinanceFuturesPlacedOrder> PlaceStopLossAsync(decimal price)
    {
        if (this.Position is null)
        {
            throw new InvalidOperationException("No position is open thus a stop loss can't be placed");
        }

        
        var SLPlacingCallResult = await this.TradingClient.PlaceOrderAsync(symbol: this.CurrencyPair.Name, side: this.Position.EntryOrder.Side.Invert(), type: FuturesOrderType.StopMarket, quantity: this.Position.EntryOrder.Quantity, stopPrice: Math.Round(price, this.NrDecimals));
        SLPlacingCallResult.ThrowIfHasError("The stop loss could not be placed");
        // // RARE EXCEPTION: InternalTradingServiceException : The stop loss could not be placed | Error: -1001: Internal error; unable to process your request. Please try again.
        // TODO retry policy //
        // Error happened due to binance exception
        // Discovered in the 12th run of a "run ultil failure integration testing

        var GetSLOrderTask = this.GetOrderAsync(SLPlacingCallResult.Data.Id);
        if (this.Position.StopLossOrder is not null)
        {
            await this.TradingClient.CancelOrderAsync(symbol: this.CurrencyPair.Name, this.Position.StopLossOrder.Id);
        }

        this.Position.StopLossOrder = await GetSLOrderTask;
        
        return SLPlacingCallResult.Data;
    }

    public bool IsInPosition() => this.Position is not null;


    //// //// ////
    

    public void Dispose()
    {
        try
        {
            this.FuturesClient.Dispose();
            this.BinanceClient.Dispose();
        }
        finally { GC.SuppressFinalize(this); }
    }
}
