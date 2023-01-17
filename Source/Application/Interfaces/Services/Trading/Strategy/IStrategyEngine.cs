using Application.Interfaces.Services.Trading;
using Binance.Net.Objects.Models.Futures;
using Domain.Models;
using Skender.Stock.Indicators;

namespace Application.Interfaces.Services.Trading.Strategy;

public interface IStrategyEngine : IDisposable, IAsyncDisposable
{
    public Task StartTradingAsync();
    
    public Task StopTradingAsync();
}
