using Infrastructure.DataAccess.Database;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Integration.Common.Fixtures;

public class FuturesTradingDbContextFactory
{
    private readonly DbContextOptions Options;
    public FuturesTradingDbContextFactory(DbContextOptions options) => this.Options = options;

    public FuturesTradingDbContext Create() => new(this.Options);
    public FuturesTradingDbContext CreateNoTrackingContext()
    {
        var dbContext = this.Create();
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking; // ensures the reads return the values from the database and NOT from memory
        return dbContext;
    }
}
