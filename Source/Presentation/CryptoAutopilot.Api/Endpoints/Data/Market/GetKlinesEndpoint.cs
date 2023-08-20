using Application.Interfaces.Services.Bybit;

using Bybit.Net.Enums;

using CryptoAutopilot.Contracts.Responses.Common;
using CryptoAutopilot.Contracts.Responses.Data.Market;

namespace CryptoAutopilot.Api.Endpoints.Data.Market;

public static class GetKlinesEndpoint
{
    public static void MapGetKlinesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Data.Market.GetAllKlines, async (string contractName, int min, IBybitUsdFuturesMarketDataProvider marketDataProvider) =>
        {
            if (!Enum.IsDefined(typeof(KlineInterval), min * 60))
            {
                return Results.BadRequest($"There is no defined {min} minutes timeframe");
            }


            var timeframe = (KlineInterval)(min * 60);
            var klines = await marketDataProvider.GetAllCandlesticksAsync(contractName, timeframe);
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
                ContractName = contractName,
                Timeframe = timeframe,
                Candlesticks = candlesticksResponses
            });
        });
    }
}
