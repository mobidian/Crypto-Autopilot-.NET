using Application.Interfaces.Services.DataAccess.Repositories;

using Infrastructure.DataAccess.Repositories;
using Infrastructure.Tests.Integration.Common.Fixtures;
using Infrastructure.Tests.Integration.DataAccess.Abstract;

namespace Infrastructure.Tests.Integration.DataAccess.TradingSignalsRepositoryTests.AbstractBase;

public abstract class TradingSignalsRepositoryTestsBase : FuturesRepositoriesTestsBase
{
    protected ITradingSignalsRepository SUT;

    protected TradingSignalsRepositoryTestsBase(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
        this.SUT = new TradingSignalsRepository(DbContextFactory.Create());
    }
}
