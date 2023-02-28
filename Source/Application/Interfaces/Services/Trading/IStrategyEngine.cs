using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;

using Domain.Models;

using Skender.Stock.Indicators;

namespace Application.Interfaces.Services.Trading;

public interface IStrategyEngine : IDisposable, IAsyncDisposable
{
    public Guid Guid { get; }

    public CurrencyPair CurrencyPair { get; }
    public KlineInterval KlineInterval { get; }


    public bool IsRunning();

    public Task StartTradingAsync();

    public Task StopTradingAsync();
}
