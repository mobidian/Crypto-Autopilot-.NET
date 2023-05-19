using Application.Interfaces.Services.DataAccess.Repositories;

using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DataAccess.Repositories;
using Infrastructure.Tests.Integration.DataAccess.Abstract;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Integration.DataAccess.FuturesPositionsRepositoryTests.AbstractBase;

public abstract class FuturesPositionsRepositoryTestsBase : FuturesRepositoriesTestsBase
{
    protected IFuturesPositionsRepository SUT;


    [SetUp]
    public virtual async Task SetUp()
    {
        this.ArrangeAssertDbContext = DbContextFactory.Create();
        this.ArrangeAssertDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking; // ensures the reads return the values from the database and NOT from memory

        this.SUT = new FuturesPositionsRepository(DbContextFactory.Create());

        await Task.CompletedTask;
    }
}
