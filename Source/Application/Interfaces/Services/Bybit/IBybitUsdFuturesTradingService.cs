using Bybit.Net.Enums;
using Bybit.Net.Objects.Models;

namespace Application.Interfaces.Services.Bybit;

public interface IBybitUsdFuturesTradingService
{
    public BybitPositionUsd? LongPosition { get; }
    public BybitPositionUsd? ShortPosition { get; }
    
    public IEnumerable<BybitUsdPerpetualOrder> LimitOrders { get; }
    public IEnumerable<BybitUsdPerpetualOrder> BuyLimitOrders { get; }
    public IEnumerable<BybitUsdPerpetualOrder> SellLimitOrders { get; }

    
    public Task<BybitPositionUsd> OpenPositionAsync(PositionSide positionSide, decimal Margin, decimal? StopLoss = null, decimal? TakeProfit = null, TriggerType tradingStopTriggerType = TriggerType.LastPrice);
    public Task<BybitPositionUsd> ModifyTradingStopAsync(PositionSide positionSide, decimal? newStopLoss = null, decimal? newTakeProfit = null, TriggerType newTradingStopTriggerType = TriggerType.LastPrice);
    public Task ClosePositionAsync(PositionSide positionSide);
    public Task CloseAllPositionsAsync();
    
    
    public Task<BybitUsdPerpetualOrder> PlaceLimitOrderAsync(OrderSide orderSide, decimal LimitPrice, decimal Margin, decimal? StopLoss = null, decimal? TakeProfit = null, TriggerType tradingStopTriggerType = TriggerType.LastPrice);
    public Task<BybitUsdPerpetualOrder> ModifyLimitOrderAsync(Guid bybitId, decimal newLimitPrice, decimal newMargin, decimal? newStopLoss = null, decimal? newTakeProfit = null, TriggerType newTradingStopTriggerType = TriggerType.LastPrice);
    public Task CancelLimitOrdersAsync(params Guid[] bybitIds);
    public Task CancelAllLimitOrdersAsync();
}
