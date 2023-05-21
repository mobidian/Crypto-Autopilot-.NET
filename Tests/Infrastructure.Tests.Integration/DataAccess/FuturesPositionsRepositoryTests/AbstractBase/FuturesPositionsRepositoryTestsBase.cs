using Application.Interfaces.Services.DataAccess.Repositories;

using Infrastructure.Services.DataAccess.Repositories;
using Infrastructure.Tests.Integration.DataAccess.Abstract;

namespace Infrastructure.Tests.Integration.DataAccess.FuturesPositionsRepositoryTests.AbstractBase;

public abstract class FuturesPositionsRepositoryTestsBase : FuturesRepositoriesTestsBase
{
    protected IFuturesPositionsRepository SUT;


    [SetUp]
    public override async Task SetUp()
    {
        await base.SetUp(); // initializes this.ArrangeAssertDbContext

        this.SUT = new FuturesPositionsRepository(DbContextFactory.Create());
    }
}
