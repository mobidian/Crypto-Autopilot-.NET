using Application.DataAccess.Repositories;

using Infrastructure.DataAccess.Repositories;
using Infrastructure.Tests.Integration.DataAccess.Abstract;

using Tests.Integration.Common.Fixtures;

namespace Infrastructure.Tests.Integration.DataAccess.FuturesOrdersRepositoryTests.AbstractBase;

public abstract class FuturesOrdersRepositoryTestsBase : FuturesRepositoriesTestsBase
{
    protected IFuturesOrdersRepository SUT;

    protected FuturesOrdersRepositoryTestsBase(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
        this.SUT = new FuturesOrdersRepository(this.DbContextFactory.Create());
    }
}
