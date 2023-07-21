using Application.Interfaces.Services.Bybit;

using Bybit.Net.Enums;

using CryptoAutopilot.Api.Endpoints.Internal.Automation.General;
using CryptoAutopilot.Contracts.Responses.Common;
using CryptoAutopilot.Contracts.Responses.Data.Market;

using Microsoft.AspNetCore.Mvc;

namespace CryptoAutopilot.Api.Endpoints.Data.Market;

public class ContractHistoryEndpoints : IEndpoints
{
    public static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
    }

    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("Data/Market/ContractHistory", async ([FromQuery] string? name, [FromQuery] int? min, IBybitUsdFuturesMarketDataProvider marketDataProvider) =>
        {
            if (name is null || min is null)
            {
                return Results.BadRequest("Contract name and timeframe are required");
            }

            if (!Enum.IsDefined(typeof(KlineInterval), min * 60))
            {
                return Results.BadRequest($"There is no defined {min} minutes timeframe");
            }


            var timeframe = (KlineInterval)(min * 60);
            var klines = await marketDataProvider.GetAllCandlesticksAsync(name, timeframe);
            var candlesticksResponses = klines.Select(x => new CandlestickResponse
            {
                ContractName = x.Symbol,
                Date = x.OpenTime,
                Open = x.OpenPrice,
                High = x.HighPrice,
                Low = x.LowPrice,
                Close = x.ClosePrice,
                Volume = x.Volume
            });

            return Results.Ok(new GetContractHistoryResponse
            {
                ContractName = name,
                Timeframe = timeframe,
                Candlesticks = candlesticksResponses
            });
        });
    }
}
