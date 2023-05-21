using Bybit.Net.Enums;
using Bybit.Net.Objects.Models;

using Domain.Models.Futures;

namespace Application.Interfaces.Services.Bybit;

public interface IBybitUsdFuturesTradingService
{
    public FuturesPosition? LongPosition { get; }
    public FuturesPosition? ShortPosition { get; }
    
    public IEnumerable<FuturesOrder> LimitOrders { get; }
    public IEnumerable<FuturesOrder> BuyLimitOrders { get; }
    public IEnumerable<FuturesOrder> SellLimitOrders { get; }
    
    
    public Task<FuturesPosition> OpenPositionAsync(PositionSide positionSide, decimal Margin, decimal? StopLoss = null, decimal? TakeProfit = null, TriggerType tradingStopTriggerType = TriggerType.LastPrice);
    public Task<FuturesPosition> ModifyTradingStopAsync(PositionSide positionSide, decimal? newStopLoss = null, decimal? newTakeProfit = null, TriggerType newTradingStopTriggerType = TriggerType.LastPrice);
    public Task ClosePositionAsync(PositionSide positionSide);
    public Task CloseAllPositionsAsync();
    
    
    public Task<FuturesOrder> PlaceLimitOrderAsync(OrderSide orderSide, decimal LimitPrice, decimal Margin, decimal? StopLoss = null, decimal? TakeProfit = null, TriggerType tradingStopTriggerType = TriggerType.LastPrice);
    public Task<FuturesOrder> ModifyLimitOrderAsync(Guid bybitId, decimal newLimitPrice, decimal newMargin, decimal? newStopLoss = null, decimal? newTakeProfit = null, TriggerType newTradingStopTriggerType = TriggerType.LastPrice);
    public Task CancelLimitOrdersAsync(params Guid[] bybitIds);
    public Task CancelAllLimitOrdersAsync();
}
