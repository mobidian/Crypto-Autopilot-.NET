using Application.Interfaces.Services.DataAccess.Repositories;

using Infrastructure.Services.DataAccess.Repositories;
using Infrastructure.Tests.Integration.DataAccess.Abstract;

namespace Infrastructure.Tests.Integration.DataAccess.FuturesOrdersRepositoryTests.AbstractBase;

public abstract class FuturesOrdersRepositoryTestsBase : FuturesRepositoriesTestsBase
{
    protected IFuturesOrdersRepository SUT;


    [SetUp]
    public override async Task SetUp()
    {
        await base.SetUp(); // initializes this.ArrangeAssertDbContext

        this.SUT = new FuturesOrdersRepository(DbContextFactory.Create());
    }
}
