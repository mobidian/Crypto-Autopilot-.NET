using System.Net;

using Application.Interfaces.Logging;
using Application.Interfaces.Services.Bybit;

using Bybit.Net.Enums;

using CryptoAutopilot.Contracts.Responses.Common;
using CryptoAutopilot.Contracts.Responses.Data.Market;
using CryptoAutopilot.DataFunctions.Extensions;
using CryptoAutopilot.DataFunctions.Functions.Abstract;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace CryptoAutopilot.DataFunctions.Functions.Data.Market;

public class GetContractHistoryFunction : TradingDataFunctionBase<GetContractHistoryFunction>
{
    public GetContractHistoryFunction(IBybitUsdFuturesMarketDataProvider marketDataProvider, ILoggerAdapter<GetContractHistoryFunction> logger) : base(marketDataProvider, logger) { }


    [Function("Data/Market/ContractHistory")]
    public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "GET")][FromQuery] HttpRequestData request, [FromQuery] string? name, [FromQuery] int? min)
    {
        if (name is null || min is null)
        {
            var badRequestResponse = request.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Contract name and timeframe are required");
            return badRequestResponse;
        }

        if (!Enum.IsDefined(typeof(KlineInterval), min * 60))
        {
            var badRequestResponse = request.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync($"There is no defined {min} minutes timeframe");
            return badRequestResponse;
        }

        var timeframe = (KlineInterval)(min * 60);
        var klines = await this.MarketDataProvider.GetAllCandlesticksAsync(name, timeframe);
        var candlesticks = klines.Select(x => new CandlestickResponse
        {
            ContractName = x.Symbol,
            Date = x.OpenTime,
            Open = x.OpenPrice,
            High = x.HighPrice,
            Low = x.LowPrice,
            Close = x.ClosePrice,
            Volume = x.Volume
        });

        var response = new GetContractHistoryResponse
        {
            ContractName = name,
            Timeframe = timeframe,
            Candlesticks = candlesticks
        };

        return await request.CreateOkJsonResponseAsync(response);
    }
}
