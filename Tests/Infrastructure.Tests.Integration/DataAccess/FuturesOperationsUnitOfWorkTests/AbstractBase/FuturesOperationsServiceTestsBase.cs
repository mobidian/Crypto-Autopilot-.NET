using Application.Interfaces.Services.DataAccess.Services;

using Infrastructure.Services.DataAccess.Repositories;
using Infrastructure.Services.DataAccess.Services;
using Infrastructure.Tests.Integration.DataAccess.Abstract;

namespace Infrastructure.Tests.Integration.DataAccess.FuturesOperationsUnitOfWorkTests.AbstractBase;

public abstract class FuturesOperationsServiceTestsBase : FuturesRepositoriesTestsBase
{
    protected IFuturesOperationsService SUT;


    [SetUp]
    public override async Task SetUp()
    {
        await base.SetUp(); // initializes this.ArrangeAssertDbContext

        
        var ctx = DbContextFactory.Create();
        var ordersRepository = new FuturesOrdersRepository(ctx);
        var positionsRepository = new FuturesPositionsRepository(ctx);

        this.SUT = new FuturesOperationsService(ctx, positionsRepository, ordersRepository);
    }
}