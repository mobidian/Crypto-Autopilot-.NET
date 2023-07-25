using CryptoAutopilot.Contracts.Responses.Common;

using Domain.Models.Futures;

namespace CryptoAutopilot.Api.Endpoints.Extensions;

public static class FuturesOrderExtensions
{
    public static FuturesOrderResponse ToResponse(this FuturesOrder futuresOrder) => new FuturesOrderResponse
    {
        BybitID = futuresOrder.BybitID,
        CurrencyPair = futuresOrder.CurrencyPair.Name,
        CreateTime = futuresOrder.CreateTime,
        UpdateTime = futuresOrder.UpdateTime,
        Side = futuresOrder.Side,
        PositionSide = futuresOrder.PositionSide,
        Type = futuresOrder.Type,
        Price = futuresOrder.Price,
        Quantity = futuresOrder.Quantity,
        StopLoss = futuresOrder.StopLoss,
        TakeProfit = futuresOrder.TakeProfit,
        TimeInForce = futuresOrder.TimeInForce,
        Status = futuresOrder.Status
    };

    public static IEnumerable<FuturesOrderResponse> ToResponses(this IEnumerable<FuturesOrder> futuresOrders) => futuresOrders.Select(x => x.ToResponse());
}
