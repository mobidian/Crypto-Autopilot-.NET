using Application.Strategies;

namespace CryptoAutopilot.Api.Services.Interfaces;

public interface IStrategiesTracker
{
    public int Count { get; }

    public void Add(IStrategyEngine strategy);
    public IEnumerable<IStrategyEngine> GetAll();
    public IStrategyEngine? Get(Guid guid);
    public void Remove(Guid guid);
    public void Clear();
}
