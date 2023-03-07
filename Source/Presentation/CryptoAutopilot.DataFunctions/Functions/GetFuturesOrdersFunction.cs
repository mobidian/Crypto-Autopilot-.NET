using System.Net;

using Application.Interfaces.Logging;
using Application.Interfaces.Services;

using CryptoAutopilot.Api.Contracts.Responses.Data;
using CryptoAutopilot.DataFunctions.Extensions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace CryptoAutopilot.DataFunctions.Functions;

public class GetFuturesOrdersFunction : DataFunctionBase
{
    public GetFuturesOrdersFunction(IFuturesTradesDBService dbService, ILoggerAdapter<GetCandlesticksFunction> logger) : base(dbService, logger) { }


    [Function("futuresorders")]
    public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get")][FromQuery] HttpRequestData request, string? currencyPair)
    {
        try
        {
            if (currencyPair is null)
            {
                var futuresOrders = await DbService.GetAllFuturesOrdersAsync();
                var resp = new GetAllFuturesOrdersResponse { FuturesOrders = futuresOrders };

                return await request.CreateOkJsonResponseAsync(resp);
            }
            else
            {
                var futuresOrders = await DbService.GetFuturesOrdersByCurrencyPairAsync(currencyPair);
                var resp = new GetFuturesOrdersByCurrencyPairResponse
                {
                    CurrencyPair = currencyPair.ToUpper(),
                    FuturesOrders = futuresOrders,
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
