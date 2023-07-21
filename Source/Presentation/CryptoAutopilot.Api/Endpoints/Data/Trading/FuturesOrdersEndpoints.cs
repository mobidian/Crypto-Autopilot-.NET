using Application.DataAccess.Repositories;

using CryptoAutopilot.Api.Endpoints.Internal.Automation.General;
using CryptoAutopilot.Contracts.Responses.Common;
using CryptoAutopilot.Contracts.Responses.Data.Trading.Orders;

using Microsoft.AspNetCore.Mvc;

namespace CryptoAutopilot.Api.Endpoints.Data.Trading;

public class FuturesOrdersEndpoints : IEndpoints
{
    public static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
    }

    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("Data/Trading/Orders", async ([FromQuery] string? contractName, IFuturesOrdersRepository ordersRepository) =>
        {
            try
            {
                if (contractName is null)
                {
                    var futuresOrders = await ordersRepository.GetAllAsync();

                    var futuresOrdersResponses = futuresOrders.Select(x => new FuturesOrderResponse
                    {
                        BybitID = x.BybitID,
                        CurrencyPair = x.CurrencyPair.Name,
                        CreateTime = x.CreateTime,
                        UpdateTime = x.UpdateTime,
                        Side = x.Side,
                        PositionSide = x.PositionSide,
                        Type = x.Type,
                        Price = x.Price,
                        Quantity = x.Quantity,
                        StopLoss = x.StopLoss,
                        TakeProfit = x.TakeProfit,
                        TimeInForce = x.TimeInForce,
                        Status = x.Status
                    });
                    var response = new GetAllFuturesOrdersResponse { FuturesOrders = futuresOrdersResponses };

                    return Results.Ok(response);
                }
                else
                {
                    var futuresOrders = await ordersRepository.GetByCurrencyPairAsync(contractName);

                    var futuresOrdersResponses = futuresOrders.Select(x => new FuturesOrderResponse
                    {
                        BybitID = x.BybitID,
                        CurrencyPair = x.CurrencyPair.Name,
                        CreateTime = x.CreateTime,
                        UpdateTime = x.UpdateTime,
                        Side = x.Side,
                        PositionSide = x.PositionSide,
                        Type = x.Type,
                        Price = x.Price,
                        Quantity = x.Quantity,
                        StopLoss = x.StopLoss,
                        TakeProfit = x.TakeProfit,
                        TimeInForce = x.TimeInForce,
                        Status = x.Status
                    });
                    var response = new GetFuturesOrdersByContractNameResponse
                    {
                        ContractName = contractName.ToUpper(),
                        FuturesOrders = futuresOrdersResponses,
                    };

                    return Results.Ok(response);
                }
            }
            catch (Exception exception)
            {
                return Results.BadRequest(exception.Message);
            }
        });
    }
}
