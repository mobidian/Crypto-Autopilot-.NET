using Application.DataAccess.Repositories;

using CryptoAutopilot.Contracts.Responses.Common;
using CryptoAutopilot.Contracts.Responses.Data.Trading.Positions;

namespace CryptoAutopilot.Api.Endpoints.Data.Trading;

public static class GetFuturesPositionsEndpoint
{
    public static void MapGetFuturesPositionsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Data.Trading.GetAllPositions, async (string? contractName, IFuturesPositionsRepository positionsRepository) =>
        {
            if (contractName is null)
            {
                var futuresPositions = await positionsRepository.GetAllAsync();

                var futuresPositionsResponses = futuresPositions.Select(x => new FuturesPositionResponse
                {
                    CryptoAutopilotId = x.CryptoAutopilotId,
                    ContractName = x.CurrencyPair.Name,
                    Side = x.Side,
                    Margin = x.Margin,
                    Leverage = x.Leverage,
                    Quantity = x.Quantity,
                    EntryPrice = x.EntryPrice,
                    ExitPrice = x.ExitPrice,
                });
                var response = new GetAllFuturesPositionsResponse { FuturesPositions = futuresPositionsResponses };

                return Results.Ok(response);
            }
            else
            {
                var futuresPositions = await positionsRepository.GetByCurrencyPairAsync(contractName);

                var futuresPositionsResponses = futuresPositions.Select(x => new FuturesPositionResponse
                {
                    CryptoAutopilotId = x.CryptoAutopilotId,
                    ContractName = x.CurrencyPair.Name,
                    Side = x.Side,
                    Margin = x.Margin,
                    Leverage = x.Leverage,
                    Quantity = x.Quantity,
                    EntryPrice = x.EntryPrice,
                    ExitPrice = x.ExitPrice
                });
                var response = new GetFuturesPositionsByContractNameResponse
                {
                    ContractName = contractName.ToUpper(),
                    FuturesPositions = futuresPositionsResponses,
                };

                return Results.Ok(response);
            }
        });
    }
}
