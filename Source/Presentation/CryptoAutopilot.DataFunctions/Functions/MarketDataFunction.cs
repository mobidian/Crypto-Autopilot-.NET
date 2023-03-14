using System.Net;

using Application.Interfaces.Logging;
using Application.Interfaces.Services.Bybit;

using Bybit.Net.Enums;

using CryptoAutopilot.Api.Contracts.Responses.Common;
using CryptoAutopilot.Api.Contracts.Responses.Data;
using CryptoAutopilot.DataFunctions.Extensions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace CryptoAutopilot.DataFunctions.Functions;

public class MarketDataFunction : TradingDataFunctionBase<MarketDataFunction>
{
    public MarketDataFunction(IBybitUsdFuturesMarketDataProvider marketDataProvider, ILoggerAdapter<MarketDataFunction> logger) : base(marketDataProvider, logger) { }

    [Function("MarketData/ContractHistory")]
    public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get")][FromQuery] HttpRequestData request, [FromQuery] string name, [FromQuery] int min)
    {
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
            CurrencyPair = x.Symbol,
            Date = x.OpenTime,
            Open = x.OpenPrice,
            High = x.HighPrice,
            Low = x.LowPrice,
            Close = x.ClosePrice,
            Volume = x.Volume
        });
           
        var response = new GetContractCandlesticksResponse
        {
            CurrencyPair = name,
            Timeframe = timeframe,
            Candlesticks = candlesticks
        };
        
        return await request.CreateOkJsonResponseAsync(response);
    }
}
