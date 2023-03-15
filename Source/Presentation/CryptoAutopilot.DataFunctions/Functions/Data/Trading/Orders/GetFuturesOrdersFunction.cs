using System.Net;

using Application.Interfaces.Logging;
using Application.Interfaces.Services;

using CryptoAutopilot.Api.Contracts.Responses.Common;
using CryptoAutopilot.Api.Contracts.Responses.Data.Trading.Orders;
using CryptoAutopilot.DataFunctions.Extensions;
using CryptoAutopilot.DataFunctions.Functions.Abstract;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace CryptoAutopilot.DataFunctions.Functions.Data.Trading.Orders;

public class GetFuturesOrdersFunction : MarketDataFunctionBase<GetFuturesOrdersFunction>
{
    public GetFuturesOrdersFunction(IFuturesTradesDBService dbService, ILoggerAdapter<GetFuturesOrdersFunction> logger) : base(dbService, logger) { }

    
    [Function("Data/Trading/FuturesOrders")]
    public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get")][FromQuery] HttpRequestData request, [FromQuery] string? contractName)
    {
        try
        {
            if (contractName is null)
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
                var futuresOrders = await DbService.GetFuturesOrdersByCurrencyPairAsync(contractName);

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
                var response = new GetFuturesOrdersByContractNameResponse
                {
                    ContractName = contractName.ToUpper(),
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
                exception.Message
            });
            return response;
        }
    }
}
