using Bogus;

using Domain.Models.Common;
using Domain.Models.Signals;

namespace Infrastructure.Tests.Integration.Common.Fakers;

public abstract class TradingSignalsFakersClass
{
    protected Faker<TradingSignal> TradingSignalGenerator = new Faker<TradingSignal>()
        .RuleFor(x => x.CryptoAutopilotId, f => Guid.NewGuid())
        .RuleFor(x => x.Source, f => f.PickRandom("Some Indicator", "Some Other Indicator", "Some Premium Indicator"))
        .RuleFor(x => x.CurrencyPair, f => new CurrencyPair(f.Finance.Currency().Code, f.Finance.Currency().Code))
        .RuleFor(x => x.Time, f => f.Date.Past())
        .RuleFor(x => x.Info, f => f.Random.Words(100));
}