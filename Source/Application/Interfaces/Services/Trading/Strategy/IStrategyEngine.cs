using Application.Interfaces.Services.Trading;
using Binance.Net.Objects.Models.Futures;
using Domain.Models;
using Skender.Stock.Indicators;

namespace Application.Interfaces.Services.Trading.Strategy;

public interface IStrategyEngine : IDisposable, IAsyncDisposable
{
    public Guid Guid { get; }

    public bool IsRunning();

    public Task StartTradingAsync();
    
    public Task StopTradingAsync();
}
