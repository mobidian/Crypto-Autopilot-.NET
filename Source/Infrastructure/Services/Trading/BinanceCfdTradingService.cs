﻿using System.Text;

using Application.Exceptions;
using Application.Interfaces.Services.Trading;

using Binance.Net.Enums;
using Binance.Net.Interfaces.Clients;
using Binance.Net.Interfaces.Clients.UsdFuturesApi;
using Binance.Net.Objects.Models.Futures;

using CryptoExchange.Net.Objects;

using Domain.Extensions;
using Domain.Models;

using Infrastructure.Extensions;

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

    public BinanceCfdTradingService(CurrencyPair currencyPair, decimal leverage, IBinanceClient binanceClient, IBinanceClientUsdFuturesApi futuresClient, IBinanceClientUsdFuturesApiTrading tradingClient, IBinanceClientUsdFuturesApiExchangeData exchangeData)
    {
        this.CurrencyPair = currencyPair;
        this.Leverage = leverage;
        this.BinanceClient = binanceClient;
        this.FuturesClient = futuresClient;
        this.TradingClient = tradingClient;
        this.ExchangeData = exchangeData;
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

    public async Task<IEnumerable<BinanceFuturesPlacedOrder>> PlaceMarketOrderAsync(OrderSide OrderSide, decimal QuoteMargin = decimal.MaxValue, decimal? StopLoss = null, decimal? TakeProfit = null)
    {
        if (this.IsInPosition())
        {
            throw new InvalidOperationException("A position is open already");
        }

        (var BaseQuantity, var CurrentPrice) = await this.Get_BaseQuantity_and_CurrentPrice_Async(QuoteMargin);
        ValidateTpSl(OrderSide, CurrentPrice, StopLoss, TakeProfit);


        var BatchOrders = this.CreateBatchOrdersForNewMarketOrder(OrderSide, StopLoss, TakeProfit, BaseQuantity);
        var PlaceOrdersCallResult = await this.TradingClient.PlaceMultipleOrdersAsync(BatchOrders);
        PlaceOrdersCallResult.ThrowIfHasError();

        this.Position = this.CreateFuturesPositionInstance(PlaceOrdersCallResult, QuoteMargin);
        
        return PlaceOrdersCallResult.Data.Select(callRes => callRes.Data);
    }
    private async Task<(decimal BaseQuantity, decimal CurrentPrice)> Get_BaseQuantity_and_CurrentPrice_Async(decimal QuoteMargin)
    {
        decimal BaseQuantity;
        decimal equityBUSD;
        decimal CurrentPrice;

        var GetCurrentPrice_Task = this.GetCurrentPriceAsync();
        equityBUSD = QuoteMargin == decimal.MaxValue ? await this.GetEquityAsync() : QuoteMargin;
        CurrentPrice = await GetCurrentPrice_Task;

        BaseQuantity = Math.Round(equityBUSD * this.Leverage / CurrentPrice, this.NrDecimals, MidpointRounding.ToZero);

        return (BaseQuantity, CurrentPrice);
    }
    private static void ValidateTpSl(OrderSide OrderSide, decimal Price, decimal? StopLoss, decimal? TakeProfit_price)
    {
        var builder = new StringBuilder();

        #region SL/TP validate positive
        if (StopLoss <= 0)
            builder.AppendLine($"Specified {nameof(StopLoss)} was negative");

        if (TakeProfit_price <= 0)
            builder.AppendLine($"Specified {nameof(TakeProfit_price)} was negative");

        if (builder.Length != 0)
            throw new InvalidOrderException(builder.Remove(builder.Length - 1, 1).ToString());
        #endregion

        #region SL/TP validate against current price
        if (OrderSide == OrderSide.Buy)
        {
            if (StopLoss >= Price)
                builder.AppendLine($"The stop loss can't be greater than or equal to the current price for a {OrderSide} order, current price was {Price} and stop loss was {StopLoss}");

            if (TakeProfit_price <= Price)
                builder.AppendLine($"The take profit can't be less greater than or equal to the current price for a {OrderSide} order, current price was {Price} and take profit was {TakeProfit_price}");
        }
        else
        {
            if (StopLoss <= Price)
                builder.AppendLine($"The stop loss can't be less greater than or equal to the current price for a {OrderSide} order, current price was {Price} and stop loss was {StopLoss}");

            if (TakeProfit_price >= Price)
                builder.AppendLine($"The take profit can't be greater than or equal to the current price for a {OrderSide} order, current price was {Price} and take profit was {TakeProfit_price}");
        }

        if (builder.Length != 0)
            throw new InvalidOrderException(builder.Remove(builder.Length - 1, 1).ToString());
        #endregion
    }
    private BinanceFuturesBatchOrder[] CreateBatchOrdersForNewMarketOrder(OrderSide OrderSide, decimal? StopLoss, decimal? TakeProfit, decimal BaseQuantity)
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
        if (StopLoss.HasValue)
        {
            BatchOrders.Add(new BinanceFuturesBatchOrder
            {
                Symbol = this.CurrencyPair.Name,
                Side = OrderSide.Invert(),
                Type = FuturesOrderType.StopMarket,
                Quantity = BaseQuantity,
                StopPrice = Math.Round(StopLoss.Value, this.NrDecimals),
            });
        }
        if (TakeProfit.HasValue)
        {
            BatchOrders.Add(new BinanceFuturesBatchOrder
            {
                Symbol = this.CurrencyPair.Name,
                Side = OrderSide.Invert(),
                Type = FuturesOrderType.TakeProfitMarket,
                Quantity = BaseQuantity,
                StopPrice = Math.Round(TakeProfit.Value, this.NrDecimals),
            });
        }

        return BatchOrders.ToArray();
    }
    private FuturesPosition CreateFuturesPositionInstance(CallResult<IEnumerable<CallResult<BinanceFuturesPlacedOrder>>> PlacedOrdersCallResult, decimal QuoteMargin)
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
            Margin = QuoteMargin,

            EntryOrder = FuturesOrders[0],
            StopLossOrder = FuturesOrders[1].Id != 0 ? FuturesOrders[1] : null,
            TakeProfitOrder = FuturesOrders[2].Id != 0 ? FuturesOrders[2] : null
        };
    }
    
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
