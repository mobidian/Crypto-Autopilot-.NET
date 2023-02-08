using System.Net;

using Application.Interfaces.Logging;
using Application.Interfaces.Services.Trading;

using CryptoAutopilot.Api.Contracts.Responses.Data;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace CryptoAutopilot.DataFunctions.Functions;

public class GetCandlesticksFunction
{
    private readonly IFuturesTradesDBService DbService;
    private readonly ILoggerAdapter<GetCandlesticksFunction> Logger;

    public GetCandlesticksFunction(IFuturesTradesDBService dbService, ILoggerAdapter<GetCandlesticksFunction> logger)
    {
        this.DbService = dbService;
        this.Logger = logger;
    }

    
    [Function("candlesticks")]
    public async Task<IResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")][FromQuery] string? currencyPair)
    {
        try
        {
            if (currencyPair is null)
            {
                var candlesticks = await this.DbService.GetAllCandlesticksAsync();
                var response = new GetAllCandlesticksResponse { Candlesticks = candlesticks };
                return Results.Ok(response);
            }
            else
            {
                var candlesticks = await this.DbService.GetCandlesticksByCurrencyPairAsync(currencyPair);
                var response = new GetCandlesticksByCurrencyPairResponse
                {
                    CurrencyPair = currencyPair.ToUpper(),
                    Candlesticks = candlesticks,
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
