using Bybit.Net.Enums;
using Bybit.Net.Objects.Models;

using Domain.Models;

namespace Infrastructure.Extensions.Bybit;

public static class BybitUsdPerpetualOrderExtensions
{
    public static FuturesOrder ToDomainObject(this BybitUsdPerpetualOrder perpetualOrder, PositionSide positionSide) => new FuturesOrder
    {
        BybitID = Guid.Parse(perpetualOrder.Id),
        CurrencyPair = perpetualOrder.Symbol,
        CreateTime = perpetualOrder.CreateTime!.Value,
        UpdateTime = perpetualOrder.UpdateTime!.Value,
        Side = perpetualOrder.Side,
        PositionSide = positionSide,
        Type = perpetualOrder.Type,
        Price = perpetualOrder.Price,
        Quantity = perpetualOrder.Quantity,
        StopLoss = perpetualOrder.StopLoss!.Value,
        TakeProfit = perpetualOrder.TakeProfit!.Value,
        TimeInForce = perpetualOrder.TimeInForce,
        Status = perpetualOrder.Status!.Value,
    };
}
