using Application.Data.Entities;

namespace Application.Interfaces.Services.Trading;

public interface IFuturesTradesDBService
{
    public Task<bool> AddCandlestickAsync(CandlestickDbEntity Candlestick);

    public Task<bool> AddFuturesOrderAsync(FuturesOrderDbEntity FuturesOrder, CandlestickDbEntity Candlestick);
}
