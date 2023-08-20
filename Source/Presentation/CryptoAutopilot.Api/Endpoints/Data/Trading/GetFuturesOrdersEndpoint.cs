using Application.DataAccess.Repositories;

using CryptoAutopilot.Api.Endpoints.Extensions;
using CryptoAutopilot.Contracts.Responses.Data.Trading.Orders;

using Microsoft.AspNetCore.Mvc;

namespace CryptoAutopilot.Api.Endpoints.Data.Trading;

public static class GetFuturesOrdersEndpoint
{
    public static void MapFuturesOrdersEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("Data/Trading/Orders", async ([FromQuery] string? contractName, IFuturesOrdersRepository ordersRepository) =>
        {
            try
            {
                if (contractName is null)
                {
                    var futuresOrders = await ordersRepository.GetAllAsync();
                    var response = new GetAllFuturesOrdersResponse { FuturesOrders = futuresOrders.ToResponses() };

                    return Results.Ok(response);
                }
                else
                {
                    var futuresOrders = await ordersRepository.GetByCurrencyPairAsync(contractName);
                    var response = new GetFuturesOrdersByContractNameResponse
                    {
                        ContractName = contractName.ToUpper(),
                        FuturesOrders = futuresOrders.ToResponses(),
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
