using Bybit.Net.Enums;

namespace Domain.Extensions;

public static class BybitEnumsExtensions
{
    public static OrderSide Invert(this OrderSide orderSide) => orderSide == OrderSide.Buy ? OrderSide.Sell : OrderSide.Buy;
    
    public static PositionSide Invert(this PositionSide positionSide) => positionSide switch
    {
        PositionSide.Buy => PositionSide.Sell,
        PositionSide.Sell => PositionSide.Buy,
        PositionSide.None => throw new ArgumentException($"{typeof(PositionSide).FullName}.None cannot be inverted"),
        _ => throw new NotImplementedException()
    };
    
    
    public static PositionSide ToPositionSide(this OrderSide orderSide) => orderSide == OrderSide.Buy ? PositionSide.Buy : PositionSide.Sell;
    
    public static PositionMode ToPositionMode(this OrderSide orderSide) => orderSide == OrderSide.Buy ? PositionMode.BothSideBuy : PositionMode.BothSideSell;
}
