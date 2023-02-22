using System.Net;

using Application.Interfaces.Logging;
using Application.Interfaces.Services.Trading;

using CryptoAutopilot.Api.Contracts.Responses.Data;
using CryptoAutopilot.DataFunctions.Extensions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace CryptoAutopilot.DataFunctions.Functions;

public class GetCandlesticksFunction : DataFunctionBase
{
    public GetCandlesticksFunction(IFuturesTradesDBService dbService, ILoggerAdapter<GetCandlesticksFunction> logger) : base(dbService, logger) { }


    [Function("candlesticks")]
    public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get")][FromQuery] HttpRequestData request, string? currencyPair)
    {
        try
        {
            if (currencyPair is null)
            {
                var candlesticks = await this.DbService.GetAllCandlesticksAsync();
                var resp = new GetAllCandlesticksResponse { Candlesticks = candlesticks };

                return await request.CreateOkJsonResponseAsync(resp);
            }
            else
            {
                var candlesticks = await this.DbService.GetCandlesticksByCurrencyPairAsync(currencyPair);
                var resp = new GetCandlesticksByCurrencyPairResponse
                {
                    CurrencyPair = currencyPair.ToUpper(),
                    Candlesticks = candlesticks,
                };

                return await request.CreateOkJsonResponseAsync(resp);
            }
        }
        catch (Exception exception)
        {
            this.Logger.LogError(exception, $"{exception.GetType().FullName}: {exception.Message}\n{exception.StackTrace}");

            var response = request.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new
            {
                Message = exception.Message
            });
            return response;
        }
    }
}
