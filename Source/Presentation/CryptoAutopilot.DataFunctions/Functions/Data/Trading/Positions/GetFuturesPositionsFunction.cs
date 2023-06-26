using System.Net;

using Application.DataAccess.Repositories;
using Application.Interfaces.Logging;

using CryptoAutopilot.Contracts.Responses.Common;
using CryptoAutopilot.Contracts.Responses.Data.Trading.Positions;
using CryptoAutopilot.DataFunctions.Extensions;
using CryptoAutopilot.DataFunctions.Functions.Abstract;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace CryptoAutopilot.DataFunctions.Functions.Data.Trading.Positions;

public class GetFuturesPositionsFunction : MarketDataFunctionBase<GetFuturesPositionsFunction>
{
    private readonly IFuturesPositionsRepository PositionsRepository;
    public GetFuturesPositionsFunction(IFuturesPositionsRepository dbService, ILoggerAdapter<GetFuturesPositionsFunction> logger) : base(logger)
    {
        this.PositionsRepository = dbService ?? throw new ArgumentNullException(nameof(dbService));
    }

    
    [Function("Data/Trading/Positions")]
    public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "GET")][FromQuery] HttpRequestData request, [FromQuery] string? contractName)
    {
        try
        {
            if (contractName is null)
            {
                var futuresPositions = await this.PositionsRepository.GetAllAsync();
                
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
                var futuresPositions = await this.PositionsRepository.GetByCurrencyPairAsync(contractName);
                
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
