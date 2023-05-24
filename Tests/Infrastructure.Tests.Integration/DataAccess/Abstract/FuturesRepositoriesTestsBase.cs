using Application.Data.Mapping;

using Domain.Models.Futures;

using Infrastructure.Internal.Extensions;
using Infrastructure.Services.DataAccess.Database;
using Infrastructure.Tests.Integration.Common.Fakers;
using Infrastructure.Tests.Integration.Common.Fixtures;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace Infrastructure.Tests.Integration.DataAccess.Abstract;

[Collection(nameof(DatabaseFixture))]
public abstract class FuturesRepositoriesTestsBase : FuturesDataFakersClass, IAsyncLifetime
{
    protected readonly FuturesTradingDbContextFactory DbContextFactory;
    protected readonly Func<Task> ClearDatabaseAsyncFunc;

    protected FuturesTradingDbContext ArrangeAssertDbContext;

    protected FuturesRepositoriesTestsBase(DatabaseFixture databaseFixture)
    {
        this.DbContextFactory = databaseFixture.DbContextFactory;
        this.ClearDatabaseAsyncFunc = databaseFixture.ClearDatabaseAsync;
        
        this.ArrangeAssertDbContext = this.DbContextFactory.CreateNoTrackingContext();
    }
    

    public virtual async Task InitializeAsync() => await Task.CompletedTask;
    public virtual async Task DisposeAsync() => await this.ClearDatabaseAsyncFunc.Invoke();


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
