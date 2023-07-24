using Application.DataAccess.Repositories;

using Infrastructure.DataAccess.Repositories;
using Infrastructure.Tests.Integration.DataAccess.Abstract;

using Tests.Integration.Common.Fixtures;

namespace Infrastructure.Tests.Integration.DataAccess.FuturesPositionsRepositoryTests.AbstractBase;

public abstract class FuturesPositionsRepositoryTestsBase : FuturesRepositoriesTestsBase
{
    protected IFuturesPositionsRepository SUT;

    protected FuturesPositionsRepositoryTestsBase(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
        this.SUT = new FuturesPositionsRepository(this.DbContextFactory.Create());
    }
}
