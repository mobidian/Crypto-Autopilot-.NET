using Application.DataAccess.Repositories;

using CryptoAutopilot.Api.Endpoints.Extensions;
using CryptoAutopilot.Contracts.Responses.Data.Trading.Orders;

namespace CryptoAutopilot.Api.Endpoints.Data.Trading;

public static class GetFuturesOrdersEndpoint
{
    public static void MapGetFuturesOrdersEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Data.Trading.GetAllOrders, async (string? contractName, IFuturesOrdersRepository ordersRepository) =>
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
        });
    }
}
