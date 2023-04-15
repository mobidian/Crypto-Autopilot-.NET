using System.Collections.Concurrent;

using Application.Exceptions;
using Application.Extensions.Bybit;
using Application.Interfaces.Services.Bybit;

using Bybit.Net.Enums;
using Bybit.Net.Objects.Models;

using Domain.Models;

using Infrastructure.Notifications.FuturesOrders;
using Infrastructure.Notifications.FuturesPositions;

using MediatR;

namespace Infrastructure.Services.Bybit;

public class BybitUsdFuturesTradingService : IBybitUsdFuturesTradingService
{
    public CurrencyPair CurrencyPair { get; }
    public decimal Leverage { get; }

    private readonly IBybitFuturesAccountDataProvider FuturesAccount;
    private readonly IBybitUsdFuturesMarketDataProvider MarketDataProvider;
    private readonly IBybitUsdFuturesTradingApiClient TradingClient;
    private readonly IMediator Mediator;
    
    public BybitUsdFuturesTradingService(CurrencyPair currencyPair, decimal leverage, IBybitFuturesAccountDataProvider futuresAccount, IBybitUsdFuturesMarketDataProvider marketDataProvider, IBybitUsdFuturesTradingApiClient tradingClient, IMediator mediator)
    {
        if (leverage is < 1 or > 100)
            throw new ArgumentException("The leverage has to be between 1 and 100 inclusive", nameof(leverage));
        
        this.CurrencyPair = currencyPair ?? throw new ArgumentNullException(nameof(currencyPair));
        this.Leverage = leverage;
        this.FuturesAccount = futuresAccount ?? throw new ArgumentNullException(nameof(futuresAccount));
        this.MarketDataProvider = marketDataProvider ?? throw new ArgumentNullException(nameof(marketDataProvider));
        this.TradingClient = tradingClient ?? throw new ArgumentNullException(nameof(tradingClient));
        this.Mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    
    //// //// ////
    
    
    private readonly IDictionary<PositionSide, (Guid CryptoAutopilotId, BybitPositionUsd Value)> positionsData = new Dictionary<PositionSide, (Guid, BybitPositionUsd)>();
    public BybitPositionUsd? LongPosition
    {
        get
        {
            this.positionsData.TryGetValue(PositionSide.Buy, out var positionData);
            return positionData.Value;
        }
    }
    public BybitPositionUsd? ShortPosition
    {
        get
        {
            this.positionsData.TryGetValue(PositionSide.Sell, out var positionData);
            return positionData.Value;
        }
    }


    private readonly IDictionary<Guid, BybitUsdPerpetualOrder> limitOrders = new ConcurrentDictionary<Guid, BybitUsdPerpetualOrder>();
    public IEnumerable<BybitUsdPerpetualOrder> LimitOrders => this.limitOrders.Values;
    public IEnumerable<BybitUsdPerpetualOrder> BuyLimitOrders => this.limitOrders.Values.Where(x => x.Side == OrderSide.Buy);
    public IEnumerable<BybitUsdPerpetualOrder> SellLimitOrders => this.limitOrders.Values.Where(x => x.Side == OrderSide.Sell);


    public async Task<BybitPositionUsd> OpenPositionAsync(PositionSide positionSide, decimal Margin, decimal? StopLoss = null, decimal? TakeProfit = null, TriggerType tradingStopTriggerType = TriggerType.LastPrice)
    {
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var quantity = Math.Round(Margin * this.Leverage / lastPrice, 2);

        var order = await this.TradingClient.PlaceOrderAsync(this.CurrencyPair.Name,
                                                             positionSide.GetEntryOrderSide(),
                                                             OrderType.Market,
                                                             quantity,
                                                             TimeInForce.ImmediateOrCancel,
                                                             false,
                                                             false,
                                                             stopLossPrice: StopLoss,
                                                             stopLossTriggerType: tradingStopTriggerType,
                                                             takeProfitPrice: TakeProfit,
                                                             takeProfitTriggerType: tradingStopTriggerType,
                                                             positionMode: positionSide.ToToPositionMode());

        
        var cryptoAutopilotId = Guid.NewGuid();
        var position = await this.FuturesAccount.GetPositionAsync(this.CurrencyPair.Name, positionSide);
        
        var existed = this.positionsData.ContainsKey(positionSide);
        this.positionsData[positionSide] = (cryptoAutopilotId, position!);
        
        if (!existed)
        {
            await this.Mediator.Publish(new PositionOpenedNotification
            {
                Position = position!.ToDomainObject(cryptoAutopilotId),
                FuturesOrders = new[] { order.ToDomainObject(positionSide) },
            });
        }
        else
        {
            await this.Mediator.Publish(new PositionUpdatedNotification
            {
                PositionCryptoAutopilotId = cryptoAutopilotId,
                UpdatedPosition = position!.ToDomainObject(cryptoAutopilotId),
                FuturesOrders = new[] { order.ToDomainObject(positionSide) },
            });
        }
        
        return position!;
    }
    public async Task<BybitPositionUsd> ModifyTradingStopAsync(PositionSide positionSide, decimal? newStopLoss = null, decimal? newTakeProfit = null, TriggerType newTradingStopTriggerType = TriggerType.LastPrice)
    {
        if (!this.positionsData.TryGetValue(positionSide, out var positionData))
            throw new InvalidOrderException($"No open {positionSide} position was found");


        var position = positionData.Value;

        await this.TradingClient.SetTradingStopAsync(this.CurrencyPair.Name,
                                                     positionSide,
                                                     stopLossPrice: newStopLoss,
                                                     stopLossQuantity: position.Quantity,
                                                     stopLossTriggerType: newTradingStopTriggerType,
                                                     takeProfitPrice: newTakeProfit,
                                                     takeProfitQuantity: position.Quantity,
                                                     takeProfitTriggerType: newTradingStopTriggerType,
                                                     positionMode: position.PositionMode);

        
        var cryptoAutopilotId = this.positionsData[positionSide]!.CryptoAutopilotId;
        var updatedPosition = await this.FuturesAccount.GetPositionAsync(this.CurrencyPair.Name, positionSide);

        this.positionsData[positionSide] = (cryptoAutopilotId, updatedPosition!);

        await this.Mediator.Publish(new PositionUpdatedNotification
        {
            PositionCryptoAutopilotId = cryptoAutopilotId,
            UpdatedPosition = updatedPosition!.ToDomainObject(cryptoAutopilotId)
        });
        
        return updatedPosition!;
    }
    public async Task ClosePositionAsync(PositionSide positionSide)
    {
        if (!this.positionsData.TryGetValue(positionSide, out var positionData))
            throw new InvalidOrderException($"No open {positionSide} position was found");


        var position = positionData.Value;

        var order = await this.TradingClient.PlaceOrderAsync(this.CurrencyPair.Name,
                                                             positionSide.GetClosingOrderSide(),
                                                             OrderType.Market,
                                                             position.Quantity,
                                                             TimeInForce.ImmediateOrCancel,
                                                             false,
                                                             false,
                                                             positionMode: position.PositionMode);

        
        (var cryptoAutopilotId, position) = this.positionsData[positionSide];
        await this.Mediator.Publish(new PositionUpdatedNotification
        {
            PositionCryptoAutopilotId = cryptoAutopilotId,
            UpdatedPosition = position.ToDomainObject(cryptoAutopilotId, order.Price),
            FuturesOrders = new[] { order.ToDomainObject(positionSide) }
        });
        
        this.positionsData.Remove(positionSide);
    }
    public async Task CloseAllPositionsAsync()
    {
        foreach (var positionSide in this.positionsData.Keys)
            await this.ClosePositionAsync(positionSide);
        
        // // TODO parallelization // //
    }

    
    public async Task<BybitUsdPerpetualOrder> PlaceLimitOrderAsync(OrderSide orderSide, decimal LimitPrice, decimal Margin, decimal? StopLoss = null, decimal? TakeProfit = null, TriggerType tradingStopTriggerType = TriggerType.LastPrice)
    {
        var quantity = Math.Round(Margin * this.Leverage / LimitPrice, 2);

        var order = await this.TradingClient.PlaceOrderAsync(this.CurrencyPair.Name,
                                                             orderSide,
                                                             OrderType.Limit,
                                                             quantity,
                                                             TimeInForce.GoodTillCanceled,
                                                             false,
                                                             false,
                                                             price: LimitPrice,
                                                             stopLossPrice: StopLoss,
                                                             stopLossTriggerType: tradingStopTriggerType,
                                                             takeProfitPrice: TakeProfit,
                                                             takeProfitTriggerType: tradingStopTriggerType,
                                                             positionMode: orderSide.ToPositionMode());


        this.limitOrders[Guid.Parse(order.Id)] = order;

        await this.Mediator.Publish(new LimitOrderPlacedNotification
        {
            LimitOrder = order.ToDomainObject(orderSide.ToPositionSide())
        });

        return order;
    }
    
    public async Task<BybitUsdPerpetualOrder> ModifyLimitOrderAsync(Guid bybitId, decimal newLimitPrice, decimal newMargin, decimal? newStopLoss = null, decimal? newTakeProfit = null, TriggerType newTradingStopTriggerType = TriggerType.LastPrice)
    {
        if (!this.limitOrders.TryGetValue(bybitId, out var oldLimitOrder))
            throw new InvalidOrderException($"No open limit order with id == {bybitId} was found");


        ValidateNewTradingStopParameters(oldLimitOrder.Side, newLimitPrice, newStopLoss, newTakeProfit);

        var newQuantity = Math.Round(newMargin * this.Leverage / newLimitPrice, 2);

        await TradingClient.ModifyOrderAsync(this.CurrencyPair.Name,
                                             oldLimitOrder.Id,
                                             newPrice: newLimitPrice,
                                             newQuantity: newQuantity,
                                             stopLossPrice: newStopLoss,
                                             stopLossTriggerType: newTradingStopTriggerType,
                                             takeProfitPrice: newTakeProfit,
                                             takeProfitTriggerType: newTradingStopTriggerType);

        
        var order = await this.TradingClient.GetOrderAsync(this.CurrencyPair.Name, oldLimitOrder.Id);
        
        this.limitOrders[bybitId] = order;
        
        await this.Mediator.Publish(new UpdatedLimitOrderNotification
        {
            BybitId = Guid.Parse(order.Id),
            UpdatedLimitOrder = order.ToDomainObject(order.Side.ToPositionSide())
        });

        return order;
    }
    private static void ValidateNewTradingStopParameters(OrderSide orderSide, decimal newLimitPrice, decimal? newStopLoss, decimal? newTakeProfit)
    {
        if (orderSide == OrderSide.Buy && (newStopLoss > newLimitPrice || newTakeProfit < newLimitPrice))
            throw new InternalTradingServiceException("Incorrect trading stop parameters for updating limit Buy order");

        if (orderSide == OrderSide.Sell && (newStopLoss < newLimitPrice || newTakeProfit > newLimitPrice))
            throw new InternalTradingServiceException("Incorrect trading stop parameters for updating limit Sell order");
    }
    
    public async Task CancelLimitOrdersAsync(params Guid[] bybitIds)
    {
        var existingIds = bybitIds.Where(this.limitOrders.ContainsKey);
        await Parallel.ForEachAsync(existingIds, async (bybitId, _) =>
        {
            await this.TradingClient.CancelOrderAsync(this.CurrencyPair.Name, bybitId.ToString());
            this.limitOrders.Remove(bybitId);
        });
        
        await this.Mediator.Publish(new CancelledLimitOrdersNotification
        {
            BybitIds = bybitIds
        });
    }

    public Task CancelAllLimitOrdersAsync() => this.CancelLimitOrdersAsync(this.limitOrders.Keys.ToArray());
}
