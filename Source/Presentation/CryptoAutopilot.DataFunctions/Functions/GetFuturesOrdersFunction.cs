using System.Net;

using Application.Interfaces.Logging;
using Application.Interfaces.Services;

using CryptoAutopilot.Api.Contracts.Responses.Common;
using CryptoAutopilot.Api.Contracts.Responses.Data;
using CryptoAutopilot.DataFunctions.Extensions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace CryptoAutopilot.DataFunctions.Functions;

public class GetFuturesOrdersFunction : DataFunctionBase<GetFuturesOrdersFunction>
{
    public GetFuturesOrdersFunction(IFuturesTradesDBService dbService, ILoggerAdapter<GetFuturesOrdersFunction> logger) : base(dbService, logger) { }
    
    
    [Function("futuresorders")]
    public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get")][FromQuery] HttpRequestData request, [FromQuery] string? currencyPair)
    {
        try
        {
            if (currencyPair is null)
            {
                var futuresOrders = await DbService.GetAllFuturesOrdersAsync();
                
                var futuresOrdersResponses = futuresOrders.Select(x => new FuturesOrderResponse
                {
                    BybitID = x.BybitID,
                    CurrencyPair = x.CurrencyPair.Name,
                    CreateTime = x.CreateTime,
                    UpdateTime = x.UpdateTime,
                    Side = x.Side,
                    PositionSide = x.PositionSide,
                    Type = x.Type,
                    Price = x.Price,
                    Quantity = x.Quantity,
                    StopLoss = x.StopLoss,
                    TakeProfit = x.TakeProfit,
                    TimeInForce = x.TimeInForce,
                    Status = x.Status
                });
                var response = new GetAllFuturesOrdersResponse { FuturesOrders = futuresOrdersResponses };

                return await request.CreateOkJsonResponseAsync(response);
            }
            else
            {
                var futuresOrders = await DbService.GetFuturesOrdersByCurrencyPairAsync(currencyPair);
                
                var futuresOrdersResponses = futuresOrders.Select(x => new FuturesOrderResponse
                {
                    BybitID = x.BybitID,
                    CurrencyPair = x.CurrencyPair.Name,
                    CreateTime = x.CreateTime,
                    UpdateTime = x.UpdateTime,
                    Side = x.Side,
                    PositionSide = x.PositionSide,
                    Type = x.Type,
                    Price = x.Price,
                    Quantity = x.Quantity,
                    StopLoss = x.StopLoss,
                    TakeProfit = x.TakeProfit,
                    TimeInForce = x.TimeInForce,
                    Status = x.Status
                });
                var response = new GetFuturesOrdersByCurrencyPairResponse
                {
                    CurrencyPair = currencyPair.ToUpper(),
                    FuturesOrders = futuresOrdersResponses,
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
                Message = exception.Message
            });
            return response;
        }
    }
}
