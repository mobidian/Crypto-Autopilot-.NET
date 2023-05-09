using Application.Interfaces.Services.DataAccess;

using Infrastructure.Services.DataAccess;
using Infrastructure.Tests.Integration.DataAccess.Abstract;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Integration.DataAccess.FuturesOrdersRepositoryTests.AbstractBase;

public abstract class FuturesOrdersRepositoryTestsBase : FuturesRepositoriesTestsBase
{
    protected IFuturesOrdersRepository SUT;


    [SetUp]
    public virtual async Task SetUp()
    {
        this.ArrangeAssertDbContext = DbContextFactory.Create();
        this.ArrangeAssertDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking; // ensures the reads return the values from the database and NOT from memory

        this.SUT = new FuturesOrdersRepository(DbContextFactory.Create());

        await Task.CompletedTask;
    }
}
