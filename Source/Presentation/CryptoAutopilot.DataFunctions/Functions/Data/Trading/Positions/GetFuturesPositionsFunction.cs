using System.Net;

using Application.Interfaces.Logging;
using Application.Interfaces.Services.DataAccess;

using CryptoAutopilot.Api.Contracts.Responses.Common;
using CryptoAutopilot.Api.Contracts.Responses.Data.Trading.Positions;
using CryptoAutopilot.DataFunctions.Extensions;
using CryptoAutopilot.DataFunctions.Functions.Abstract;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace CryptoAutopilot.DataFunctions.Functions.Data.Trading.Positions;

public class GetFuturesPositionsFunction : MarketDataFunctionBase<GetFuturesPositionsFunction>
{
    public GetFuturesPositionsFunction(IFuturesTradesDBService dbService, ILoggerAdapter<GetFuturesPositionsFunction> logger) : base(dbService, logger) { }

    
    [Function("Data/Trading/Positions")]
    public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get")][FromQuery] HttpRequestData request, [FromQuery] string? contractName)
    {
        try
        {
            if (contractName is null)
            {
                var futuresPositions = await DbService.GetAllFuturesPositionsAsync();
                
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

                return await request.CreateOkJsonResponseAsync(response);
            }
            else
            {
                var futuresPositions = await DbService.GetFuturesPositionsByCurrencyPairAsync(contractName);
                
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

                return await request.CreateOkJsonResponseAsync(response);
            }
        }
        catch (Exception exception)
        {
            this.Logger.LogError(exception, $"{exception.GetType().FullName}: {exception.Message}\n{exception.StackTrace}");

            var response = request.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new
            {
                exception.Message
            });
            return response;
        }
    }
}
