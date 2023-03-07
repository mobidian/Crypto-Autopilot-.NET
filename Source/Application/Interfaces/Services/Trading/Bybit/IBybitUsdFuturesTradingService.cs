using Bybit.Net.Enums;
using Bybit.Net.Objects.Models;

namespace Application.Interfaces.Services.Trading.Bybit;

public interface IBybitUsdFuturesTradingService
{
    public BybitPositionUsd? LongPosition { get; }
    public BybitPositionUsd? ShortPosition { get; }

    public BybitUsdPerpetualOrder? BuyLimitOrder { get; }
    public BybitUsdPerpetualOrder? SellLimitOrder { get; }


    public Task OpenPositionAsync(PositionSide positionSide, decimal Margin, decimal? StopLoss = null, decimal? TakeProfit = null, TriggerType tradingStopTriggerType = TriggerType.LastPrice);
    public Task ModifyTradingStopAsync(PositionSide positionSide, decimal? newStopLoss = null, decimal? newTakeProfit = null, TriggerType newTradingStopTriggerType = TriggerType.LastPrice);
    public Task ClosePositionAsync(PositionSide positionSide);


    public Task PlaceLimitOrderAsync(OrderSide orderSide, decimal LimitPrice, decimal Margin, decimal? StopLoss = null, decimal? TakeProfit = null, TriggerType tradingStopTriggerType = TriggerType.LastPrice);
    public Task ModifyLimitOrderAsync(OrderSide orderSide, decimal newLimitPrice, decimal newMargin, decimal? newStopLoss = null, decimal? newTakeProfit = null, TriggerType newTradingStopTriggerType = TriggerType.LastPrice);
    public Task CancelLimitOrderAsync(OrderSide orderSide);
}
