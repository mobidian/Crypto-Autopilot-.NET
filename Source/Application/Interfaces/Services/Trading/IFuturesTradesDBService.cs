using Binance.Net.Objects.Models.Futures;

using Domain.Models;

namespace Application.Interfaces.Services.Trading;

public interface IFuturesTradesDBService
{
    public Task AddCandlestickAsync(Candlestick Candlestick);
    
    public Task AddFuturesOrderAsync(BinanceFuturesOrder FuturesOrder, Candlestick Candlestick);
}
