using System.Net;

using Application.Interfaces.Logging;
using Application.Interfaces.Services.Trading;

using CryptoAutopilot.Api.Contracts.Responses.Data;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace CryptoAutopilot.DataFunctions.Functions;

public class GetFuturesOrdersFunction
{
    private readonly IFuturesTradesDBService DbService;
    private readonly ILoggerAdapter<GetCandlesticksFunction> Logger;

    public GetFuturesOrdersFunction(IFuturesTradesDBService dbService, ILoggerAdapter<GetCandlesticksFunction> logger)
    {
        this.DbService = dbService;
        this.Logger = logger;
    }


    [Function("futuresorders")]
    public async Task<IResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")][FromQuery] string? currencyPair)
    {
        try
        {
            if (currencyPair is null)
            {
                var futuresOrders = await DbService.GetAllFuturesOrdersAsync();
                var response = new GetAllFuturesOrdersResponse { FuturesOrders = futuresOrders };
                return Results.Ok(response);
            }
            else
            {
                var futuresOrders = await DbService.GetFuturesOrdersByCurrencyPairAsync(currencyPair);
                var response = new GetFuturesOrdersByCurrencyPairResponse
                {
                    CurrencyPair = currencyPair.ToUpper(),
                    FuturesOrders = futuresOrders,
                };
                return Results.Ok(response);
            }
        }
        catch (Exception exception)
        {
            this.Logger.LogError(exception, $"{exception.GetType().FullName}: {exception.Message}\n{exception.StackTrace}");
            return Results.Problem(exception.Message, exception.StackTrace, (int)HttpStatusCode.InternalServerError, "Exception while trying to query database", exception.GetType().FullName);
        }
    }
}
