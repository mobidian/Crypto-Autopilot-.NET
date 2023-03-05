using Bybit.Net.Enums;

namespace Domain.Extensions.Bybit;

public static class BybitOrderSideExtensions
{
    public static OrderSide Invert(this OrderSide orderSide) => orderSide == OrderSide.Buy ? OrderSide.Sell : OrderSide.Buy;

    public static PositionSide ToPositionSide(this OrderSide orderSide) => orderSide == OrderSide.Buy ? PositionSide.Buy : PositionSide.Sell;

    public static PositionMode ToPositionMode(this OrderSide orderSide) => orderSide == OrderSide.Buy ? PositionMode.BothSideBuy : PositionMode.BothSideSell;
}
