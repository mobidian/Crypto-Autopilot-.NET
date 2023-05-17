using Bybit.Net.Objects.Models;

using Domain.Models.Orders;

namespace Application.Extensions.Bybit;

public static class BybitPositionUsdExtensions
{
    public static FuturesPosition ToDomainObject(this BybitPositionUsd position, Guid CryptoAutopilotId) => new FuturesPosition
    {
        CryptoAutopilotId = CryptoAutopilotId,
        CurrencyPair = position.Symbol,
        Side = position.Side,
        Margin = position.PositionMargin,
        Leverage = position.Leverage,
        Quantity = position.Quantity,
        EntryPrice = position.EntryPrice,
        StopLoss = position.StopLoss,
        TakeProfit = position.TakeProfit,
    };
}
