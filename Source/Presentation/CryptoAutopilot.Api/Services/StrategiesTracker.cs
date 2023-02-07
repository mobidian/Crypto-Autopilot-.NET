using System.Collections.Concurrent;

using Application.Interfaces.Services.Trading.Strategy;

using Presentation.Api.Services.Interfaces;

namespace Presentation.Api.Services;

public class StrategiesTracker : IStrategiesTracker
{
    private readonly ConcurrentDictionary<Guid, IStrategyEngine> Strategies = new();

    public int Count => this.Strategies.Count;


    public void Add(IStrategyEngine strategy) => this.Strategies.TryAdd(strategy.Guid, strategy);

    public IEnumerable<IStrategyEngine> GetAll() => this.Strategies.Values;

    public IStrategyEngine? Get(Guid guid) => this.Strategies.GetValueOrDefault(guid);

    public void Remove(Guid guid) => this.Strategies.TryRemove(guid, out _);

    public void Clear() => this.Strategies.Clear();
}
