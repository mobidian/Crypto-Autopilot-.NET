using Bybit.Net.Enums;

namespace Infrastructure.Extensions.Bybit;

public static class BybitPositionSideExtensions
{
    public static PositionSide Invert(this PositionSide positionSide) => positionSide switch
    {
        PositionSide.Buy => PositionSide.Sell,
        PositionSide.Sell => PositionSide.Buy,
        PositionSide.None => throw new ArgumentException($"{typeof(PositionSide).FullName}.None cannot be inverted"),
        _ => throw new NotImplementedException()
    };

    public static OrderSide GetEntryOrderSide(this PositionSide positionSide) => positionSide == PositionSide.Buy ? OrderSide.Buy : OrderSide.Sell;

    public static OrderSide GetClosingOrderSide(this PositionSide positionSide) => positionSide == PositionSide.Buy ? OrderSide.Sell : OrderSide.Buy;

    public static PositionMode ToToPositionMode(this PositionSide positionSide) => positionSide == PositionSide.Buy ? PositionMode.BothSideBuy : PositionMode.BothSideSell;
}
