using Application.DataAccess.Repositories;

using CryptoAutopilot.Api.Endpoints.Internal.Automation.General;
using CryptoAutopilot.Contracts.Responses.Common;
using CryptoAutopilot.Contracts.Responses.Data.Trading.Positions;

using Microsoft.AspNetCore.Mvc;

namespace CryptoAutopilot.Api.Endpoints.Data.Trading;

public class FuturesPositionsEndpoints : IEndpoints
{
    public static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
    }

    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("Data/Trading/Positions", async ([FromQuery] string? contractName, IFuturesPositionsRepository positionsRepository) =>
        {
            try
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
            }
            catch (Exception exception)
            {
                return Results.BadRequest(exception.Message);
            }
        });
    }
}
