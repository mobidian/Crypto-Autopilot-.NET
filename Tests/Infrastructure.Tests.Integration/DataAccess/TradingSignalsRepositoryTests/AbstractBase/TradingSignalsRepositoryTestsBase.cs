using Application.Interfaces.Services.DataAccess.Repositories;

using Domain.Models.Common;
using Domain.Models.Signals;

using Infrastructure.Services.DataAccess.Repositories;
using Infrastructure.Tests.Integration.DataAccess.Abstract;

namespace Infrastructure.Tests.Integration.DataAccess.TradingSignalsRepositoryTests.AbstractBase;

public abstract class TradingSignalsRepositoryTestsBase : FuturesRepositoriesTestsBase
{
    protected ITradingSignalsRepository SUT;


    protected Faker<TradingSignal> TradingSignalGenerator = new Faker<TradingSignal>()
        .RuleFor(x => x.CryptoAutopilotId, f => Guid.NewGuid())
        .RuleFor(x => x.Source, f => f.PickRandom("Some Indicator", "Some Other Indicator", "Some Premium Indicator"))
        .RuleFor(x => x.CurrencyPair, f => new CurrencyPair(f.Finance.Currency().Code, f.Finance.Currency().Code))
        .RuleFor(x => x.Time, f => f.Date.Past())
        .RuleFor(x => x.Info, f => f.Random.Words(100));
    
    
    [SetUp]
    public override async Task SetUp()
    {
        await base.SetUp(); // initializes this.ArrangeAssertDbContext

        this.SUT = new TradingSignalsRepository(DbContextFactory.Create());
    }
}
