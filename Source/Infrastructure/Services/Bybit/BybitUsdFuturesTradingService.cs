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
    
    
    private readonly IDictionary<PositionSide, FuturesPosition> positions = new Dictionary<PositionSide, FuturesPosition>();
    public FuturesPosition? LongPosition
    {
        get
        {
            this.positions.TryGetValue(PositionSide.Buy, out var position);
            return position;
        }
    }
    public FuturesPosition? ShortPosition
    {
        get
        {
            this.positions.TryGetValue(PositionSide.Sell, out var position);
            return position;
        }
    }


    private readonly IDictionary<Guid, FuturesOrder> limitOrders = new ConcurrentDictionary<Guid, FuturesOrder>();
    public IEnumerable<FuturesOrder> LimitOrders => this.limitOrders.Values;
    public IEnumerable<FuturesOrder> BuyLimitOrders => this.limitOrders.Values.Where(x => x.Side == OrderSide.Buy);
    public IEnumerable<FuturesOrder> SellLimitOrders => this.limitOrders.Values.Where(x => x.Side == OrderSide.Sell);


    public async Task<FuturesPosition> OpenPositionAsync(PositionSide positionSide, decimal Margin, decimal? StopLoss = null, decimal? TakeProfit = null, TriggerType tradingStopTriggerType = TriggerType.LastPrice)
    {
        var lastPrice = await this.MarketDataProvider.GetLastPriceAsync(this.CurrencyPair.Name);
        var quantity = Math.Round(Margin * this.Leverage / lastPrice, 2);

        var bybitOrder = await this.TradingClient.PlaceOrderAsync(this.CurrencyPair.Name,
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

        var bybitPosition = await this.FuturesAccount.GetPositionAsync(this.CurrencyPair.Name, positionSide);

        var cryptoAutopilotId = Guid.NewGuid();
        var order = bybitOrder.ToDomainObject(positionSide);
        var position = bybitPosition!.ToDomainObject(cryptoAutopilotId);


        var existed = this.positions.ContainsKey(positionSide);
        this.positions[positionSide] = position;
        
        if (!existed)
        {
            await this.Mediator.Publish(new PositionOpenedNotification
            {
                Position = position,
                FuturesOrders = new[] { order },
            });
        }
        else
        {
            await this.Mediator.Publish(new PositionUpdatedNotification
            {
                PositionCryptoAutopilotId = cryptoAutopilotId,
                UpdatedPosition = position,
                FuturesOrders = new[] { order },
            });
        }
        
        return position;
    }
    public async Task<FuturesPosition> ModifyTradingStopAsync(PositionSide positionSide, decimal? newStopLoss = null, decimal? newTakeProfit = null, TriggerType newTradingStopTriggerType = TriggerType.LastPrice)
    {
        if (!this.positions.TryGetValue(positionSide, out var position))
            throw new InvalidOrderException($"No open {positionSide} position was found");

        
        await this.TradingClient.SetTradingStopAsync(this.CurrencyPair.Name,
                                                     positionSide,
                                                     stopLossPrice: newStopLoss,
                                                     stopLossQuantity: position.Quantity,
                                                     stopLossTriggerType: newTradingStopTriggerType,
                                                     takeProfitPrice: newTakeProfit,
                                                     takeProfitQuantity: position.Quantity,
                                                     takeProfitTriggerType: newTradingStopTriggerType,
                                                     positionMode: position.Side.ToToPositionMode());

        var bybitUpdatedPosition = await this.FuturesAccount.GetPositionAsync(this.CurrencyPair.Name, positionSide);

        var cryptoAutopilotId = this.positions[positionSide]!.CryptoAutopilotId;
        var updatedPosition = bybitUpdatedPosition!.ToDomainObject(cryptoAutopilotId);

        this.positions[positionSide] = updatedPosition;

        await this.Mediator.Publish(new PositionUpdatedNotification
        {
            PositionCryptoAutopilotId = cryptoAutopilotId,
            UpdatedPosition = updatedPosition
        });
        
        return updatedPosition;
    }
    public async Task ClosePositionAsync(PositionSide positionSide)
    {
        if (!this.positions.TryGetValue(positionSide, out var position))
            throw new InvalidOrderException($"No open {positionSide} position was found");


        var bybitOrder = await this.TradingClient.PlaceOrderAsync(this.CurrencyPair.Name,
                                                             positionSide.GetClosingOrderSide(),
                                                             OrderType.Market,
                                                             position.Quantity,
                                                             TimeInForce.ImmediateOrCancel,
                                                             false,
                                                             false,
                                                             positionMode: position.Side.ToToPositionMode());

        
        var cryptoAutopilotId = this.positions[positionSide].CryptoAutopilotId;
        var order = bybitOrder.ToDomainObject(positionSide);
        position.ExitPrice = bybitOrder.Price;

        await this.Mediator.Publish(new PositionUpdatedNotification
        {
            PositionCryptoAutopilotId = cryptoAutopilotId,
            UpdatedPosition = position,
            FuturesOrders = new[] { order }
        });
        
        this.positions.Remove(positionSide);
    }
    public async Task CloseAllPositionsAsync()
    {
        foreach (var positionSide in this.positions.Keys)
            await this.ClosePositionAsync(positionSide);
        
        // // TODO parallelization // //
    }

    
    public async Task<FuturesOrder> PlaceLimitOrderAsync(OrderSide orderSide, decimal LimitPrice, decimal Margin, decimal? StopLoss = null, decimal? TakeProfit = null, TriggerType tradingStopTriggerType = TriggerType.LastPrice)
    {
        var quantity = Math.Round(Margin * this.Leverage / LimitPrice, 2);

        var bybitOrder = await this.TradingClient.PlaceOrderAsync(this.CurrencyPair.Name,
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


        var order = bybitOrder.ToDomainObject(orderSide.ToPositionSide());
        this.limitOrders[Guid.Parse(bybitOrder.Id)] = order;

        await this.Mediator.Publish(new LimitOrderPlacedNotification
        {
            LimitOrder = order
        });

        return order;
    }
    
    public async Task<FuturesOrder> ModifyLimitOrderAsync(Guid bybitId, decimal newLimitPrice, decimal newMargin, decimal? newStopLoss = null, decimal? newTakeProfit = null, TriggerType newTradingStopTriggerType = TriggerType.LastPrice)
    {
        if (!this.limitOrders.TryGetValue(bybitId, out var oldLimitOrder))
            throw new InvalidOrderException($"No open limit order with id == {bybitId} was found");


        ValidateNewTradingStopParameters(oldLimitOrder.Side, newLimitPrice, newStopLoss, newTakeProfit);

        var newQuantity = Math.Round(newMargin * this.Leverage / newLimitPrice, 2);
        
        await TradingClient.ModifyOrderAsync(this.CurrencyPair.Name,
                                             oldLimitOrder.BybitID.ToString(),
                                             newPrice: newLimitPrice,
                                             newQuantity: newQuantity,
                                             stopLossPrice: newStopLoss,
                                             stopLossTriggerType: newTradingStopTriggerType,
                                             takeProfitPrice: newTakeProfit,
                                             takeProfitTriggerType: newTradingStopTriggerType);

        
        var updatedBybitOrder = await this.TradingClient.GetOrderAsync(this.CurrencyPair.Name, oldLimitOrder.BybitID.ToString());
        var updatedOrder = updatedBybitOrder.ToDomainObject(updatedBybitOrder.Side.ToPositionSide());
        
        this.limitOrders[bybitId] = updatedOrder;
        
        await this.Mediator.Publish(new UpdatedLimitOrderNotification
        {
            BybitId = updatedOrder.BybitID,
            UpdatedLimitOrder = updatedOrder
        });

        return updatedOrder;
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
