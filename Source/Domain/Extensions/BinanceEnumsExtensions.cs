using Binance.Net.Enums;

namespace Domain.Extensions;

public static class BinanceEnumsExtensions
{
    public static OrderSide Invert(this OrderSide orderSide) => orderSide == OrderSide.Buy ? OrderSide.Sell : OrderSide.Buy;

    public static PositionSide Invert(this PositionSide positionSide) => positionSide switch
    {
        PositionSide.Long => PositionSide.Short,
        PositionSide.Short => PositionSide.Long,
        PositionSide.Both => PositionSide.Both
    };
}
