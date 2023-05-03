using Infrastructure.Database.Contexts;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Common;

public class FuturesTradingDbContextFactory
{
    private readonly string ConnectionString;
    public FuturesTradingDbContextFactory(string connectionString) => this.ConnectionString = connectionString;

    public FuturesTradingDbContext Create()
    {
        var optionsBuilder = new DbContextOptionsBuilder();
        optionsBuilder.UseSqlServer(this.ConnectionString);

        return new FuturesTradingDbContext(optionsBuilder.Options);
    }
}