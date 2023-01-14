namespace Application.Interfaces.Services.Trading;

public interface IPoolTradingService : IDisposable
{
    public int NrStrategiesStarted { get; }
    public int NrStrategiesRunning { get; }

    public Task StartTradingAsync();
}
