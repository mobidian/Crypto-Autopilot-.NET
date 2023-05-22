using Application.Data.Mapping;

using Domain.Models.Futures;

using Infrastructure.Database;
using Infrastructure.Internal.Extensions;
using Infrastructure.Tests.Integration.AbstractBases;
using Infrastructure.Tests.Integration.Common;
using Infrastructure.Tests.Integration.DataAccess.Common;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace Infrastructure.Tests.Integration.DataAccess.Abstract;

[Collection(nameof(DatabaseFixture))]
public abstract class FuturesRepositoriesTestsBase : FuturesDataFakersClass, IAsyncLifetime
{
    protected readonly FuturesTradingDbContextFactory DbContextFactory;
    protected readonly Func<Task> ClearDatabaseAsyncFunc;

    protected FuturesRepositoriesTestsBase(DatabaseFixture databaseFixture)
    {
        this.DbContextFactory = databaseFixture.DbContextFactory;
        this.ClearDatabaseAsyncFunc = databaseFixture.ClearDatabaseAsync;
    }

    
    protected FuturesTradingDbContext ArrangeAssertDbContext = default!;
    public virtual async Task InitializeAsync()
    {
        this.ArrangeAssertDbContext = this.DbContextFactory.Create();
        this.ArrangeAssertDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking; // ensures the reads return the values from the database and NOT from memory

        await Task.CompletedTask;
    }
    public virtual async Task DisposeAsync()
    {
        await this.ClearDatabaseAsyncFunc.Invoke();
    }


    protected async Task InsertRelatedPositionAndOrdersAsync(FuturesPosition position, IEnumerable<FuturesOrder> orders)
    {
        using var _ = await this.ArrangeAssertDbContext.Database.BeginTransactionalOperationAsync();

        var positionDbEntity = position.ToDbEntity();
        await this.ArrangeAssertDbContext.FuturesPositions.AddAsync(positionDbEntity);
        await this.ArrangeAssertDbContext.SaveChangesAsync();

        var futuresOrderDbEntities = orders.Select(x =>
        {
            var entity = x.ToDbEntity();
            entity.PositionId = positionDbEntity.Id;
            return entity;
        });
        await this.ArrangeAssertDbContext.FuturesOrders.AddRangeAsync(futuresOrderDbEntities);
        await this.ArrangeAssertDbContext.SaveChangesAsync();
    }
}
