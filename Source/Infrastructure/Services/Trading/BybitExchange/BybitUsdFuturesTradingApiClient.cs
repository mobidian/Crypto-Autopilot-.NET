using Application.Interfaces.Services.Trading.BybitExchange;

using Bybit.Net.Enums;
using Bybit.Net.Interfaces.Clients.UsdPerpetualApi;
using Bybit.Net.Objects.Models;

using Infrastructure.Extensions;
using Infrastructure.Extensions.Bybit;

namespace Infrastructure.Services.Trading.BybitExchange;

public class BybitUsdFuturesTradingApiClient : IBybitUsdFuturesTradingApiClient
{
    private readonly IBybitClientUsdPerpetualApiTrading FuturesClient;

    public BybitUsdFuturesTradingApiClient(IBybitClientUsdPerpetualApiTrading tradingClient)
    {
        this.FuturesClient = tradingClient ?? throw new ArgumentNullException(nameof(tradingClient));
    }


    public async Task<BybitUsdPerpetualOrder> GetOrderAsync(string symbol, string OrderID)
    {
        var callResult = await this.FuturesClient.GetOpenOrderRealTimeAsync(symbol, OrderID);
        callResult.ThrowIfHasError();
        return callResult.Data;
    }

    public async Task<BybitUsdPerpetualOrder> PlaceOrderAsync(string symbol, OrderSide side, OrderType type, decimal quantity, TimeInForce timeInForce, bool reduceOnly, bool closeOnTrigger, decimal? price = null, string? clientOrderId = null, decimal? takeProfitPrice = null, decimal? stopLossPrice = null, TriggerType? takeProfitTriggerType = null, TriggerType? stopLossTriggerType = null, PositionMode? positionMode = null, long? receiveWindow = null)
    {
        var callResult = await this.FuturesClient.PlaceOrderAsync(symbol, side, type, quantity, timeInForce, reduceOnly, closeOnTrigger, price, clientOrderId, takeProfitPrice, stopLossPrice, takeProfitTriggerType, stopLossTriggerType, positionMode, receiveWindow);
        callResult.ThrowIfHasError();
        return callResult.Data;
    }
    
    public async Task SetTradingStopAsync(string symbol, PositionSide side, decimal? takeProfitPrice = null, decimal? stopLossPrice = null, decimal? trailingStopPrice = null, TriggerType? takeProfitTriggerType = null, TriggerType? stopLossTriggerType = null, decimal? takeProfitQuantity = null, decimal? stopLossQuantity = null, PositionMode? positionMode = null, long? receiveWindow = null)
    {
        var callResult = await this.FuturesClient.SetTradingStopAsync(symbol, side, takeProfitPrice, stopLossPrice, trailingStopPrice, takeProfitTriggerType, stopLossTriggerType, takeProfitQuantity, stopLossQuantity, positionMode, receiveWindow);
        callResult.ThrowIfHasError();
    }
    
    public async Task<BybitOrderId> ModifyOrderAsync(string symbol, string? orderId = null, string? clientOrderId = null, decimal? newPrice = null, decimal? newQuantity = null, decimal? takeProfitPrice = null, decimal? stopLossPrice = null, TriggerType? takeProfitTriggerType = null, TriggerType? stopLossTriggerType = null, long? receiveWindow = null)
    {
        var callResult = await this.FuturesClient.ModifyOrderAsync(symbol, orderId, clientOrderId, newPrice, newQuantity, takeProfitPrice, stopLossPrice, takeProfitTriggerType, stopLossTriggerType, receiveWindow);
        callResult.ThrowIfHasError();
        return callResult.Data;
    }
    
    public async Task<BybitOrderId> CancelOrderAsync(string symbol, string? orderId = null, string? clientOrderId = null, long? receiveWindow = null)
    {
        var callResult = await this.FuturesClient.CancelOrderAsync(symbol, orderId, clientOrderId, receiveWindow);
        callResult.ThrowIfHasError();
        return callResult.Data;
    }
    
    public async Task<IEnumerable<string>> CancelAllOrdersAsync(string symbol, long? receiveWindow = null)
    {
        var callResult = await this.FuturesClient.CancelAllOrdersAsync(symbol, receiveWindow);
        callResult.ThrowIfHasError();
        return callResult.Data;
    }

    public async Task<BybitUsdPerpetualOrder> CloseOrderAsync(string symbol, string OrderID)
    {
        var perpetualOrder = await this.GetOrderAsync(symbol, OrderID);
        return await this.CloseOrderAsync(perpetualOrder);
    }
    public async Task<BybitUsdPerpetualOrder> CloseOrderAsync(BybitUsdPerpetualOrder perpetualOrder)
    {
        if (perpetualOrder.Status is not OrderStatus.Created and not OrderStatus.Filled)
            throw new ArgumentException($"Can't close a {perpetualOrder.Status} order");
        
        return await this.PlaceOrderAsync(perpetualOrder.Symbol, perpetualOrder.Side.Invert(), OrderType.Market, perpetualOrder.Quantity, TimeInForce.ImmediateOrCancel, false, false, positionMode: perpetualOrder.Side.ToPositionMode());   
    }
}
