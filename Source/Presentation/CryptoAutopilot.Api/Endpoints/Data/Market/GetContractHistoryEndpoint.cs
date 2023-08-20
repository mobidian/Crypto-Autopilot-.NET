using Application.Interfaces.Services.Bybit;

using Bybit.Net.Enums;

using CryptoAutopilot.Contracts.Responses.Common;
using CryptoAutopilot.Contracts.Responses.Data.Market;

using Microsoft.AspNetCore.Mvc;

namespace CryptoAutopilot.Api.Endpoints.Data.Market;

public static class GetContractHistoryEndpoint
{
    public static void MapGetContractHistoryEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("Data/Market/ContractHistory", async ([FromQuery] string? name, [FromQuery] int? min, IBybitUsdFuturesMarketDataProvider marketDataProvider) =>
        {
            try
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
            }
            catch (Exception exception)
            {
                return Results.BadRequest(exception.Message);
            }
        });
    }
}
