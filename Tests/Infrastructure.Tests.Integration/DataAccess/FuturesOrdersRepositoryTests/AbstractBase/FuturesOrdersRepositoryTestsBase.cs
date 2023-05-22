using Application.Interfaces.Services.DataAccess.Repositories;

using Infrastructure.Services.DataAccess.Repositories;
using Infrastructure.Tests.Integration.Common.Fixtures;
using Infrastructure.Tests.Integration.DataAccess.Abstract;

namespace Infrastructure.Tests.Integration.DataAccess.FuturesOrdersRepositoryTests.AbstractBase;

public abstract class FuturesOrdersRepositoryTestsBase : FuturesRepositoriesTestsBase
{
    protected IFuturesOrdersRepository SUT;

    protected FuturesOrdersRepositoryTestsBase(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
        this.SUT = new FuturesOrdersRepository(this.DbContextFactory.Create());
    }
}
