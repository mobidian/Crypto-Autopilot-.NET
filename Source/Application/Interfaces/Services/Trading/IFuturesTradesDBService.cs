using Application.Data.Entities;

using Binance.Net.Objects.Models.Futures;

using Domain.Models;

namespace Application.Interfaces.Services.Trading;

public interface IFuturesTradesDBService
{
    public Task<bool> AddCandlestickAsync(Candlestick Candlestick);
    
    public Task<bool> AddFuturesOrderAsync(BinanceFuturesOrder FuturesOrder, Candlestick Candlestick);
}
