using Bybit.Net.Enums;
using Bybit.Net.Objects.Models;

namespace Application.Interfaces.Services.Bybit;

public interface IBybitUsdFuturesTradingApiClient
{
    public Task<BybitUsdPerpetualOrder> GetOrderAsync(string symbol, Guid orderId);

    public Task<BybitUsdPerpetualOrder> PlaceOrderAsync(string symbol, OrderSide side, OrderType type, decimal quantity, TimeInForce timeInForce, bool reduceOnly, bool closeOnTrigger, decimal? price = null, string? clientOrderId = null, decimal? takeProfitPrice = null, decimal? stopLossPrice = null, TriggerType? takeProfitTriggerType = null, TriggerType? stopLossTriggerType = null, PositionMode? positionMode = null, long? receiveWindow = null);

    public Task<BybitOrderId> ModifyOrderAsync(string symbol, string? orderId = null, string? clientOrderId = null, decimal? newPrice = null, decimal? newQuantity = null, decimal? takeProfitPrice = null, decimal? stopLossPrice = null, TriggerType? takeProfitTriggerType = null, TriggerType? stopLossTriggerType = null, long? receiveWindow = null);

    public Task SetTradingStopAsync(string symbol, PositionSide side, decimal? takeProfitPrice = null, decimal? stopLossPrice = null, decimal? trailingStopPriceDistance = null, TriggerType? takeProfitTriggerType = null, TriggerType? stopLossTriggerType = null, decimal? takeProfitQuantity = null, decimal? stopLossQuantity = null, PositionMode? positionMode = null, long? receiveWindow = null);

    public Task<BybitOrderId> CancelOrderAsync(string symbol, string? orderId = null, string? clientOrderId = null, long? receiveWindow = null);
    public Task<IEnumerable<string>> CancelAllOrdersAsync(string symbol, long? receiveWindow = null);

    public Task<BybitUsdPerpetualOrder> CloseOrderAsync(string symbol, Guid orderId);
    public Task<BybitUsdPerpetualOrder> CloseOrderAsync(BybitUsdPerpetualOrder perpetualOrder);
}
