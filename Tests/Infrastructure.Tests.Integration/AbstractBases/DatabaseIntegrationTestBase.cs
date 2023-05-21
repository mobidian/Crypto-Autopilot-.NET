using Infrastructure.Database;
using Infrastructure.Tests.Integration.DataAccess.Common;

using Microsoft.EntityFrameworkCore;

using Respawn;

namespace Infrastructure.Tests.Integration.AbstractBases;

public class DatabaseIntegrationTestBase
{
    protected const string ConnectionString = """Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TradingHistoryDB-TestDatabase;Integrated Security=True;Connect Timeout=60;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False""";
    protected static FuturesTradingDbContextFactory DbContextFactory = new(ConnectionString);
    protected static Respawner DbRespawner = default!;

    protected FuturesTradingDbContext ArrangeAssertDbContext = default!;


    [OneTimeSetUp]
    public static async Task OneTimeSetUp()
    {
        var dbContext = DbContextFactory.Create();
        await dbContext.Database.EnsureCreatedAsync();

        DbRespawner = await Respawner.CreateAsync(ConnectionString, new RespawnerOptions { CheckTemporalTables = true });
        await DbRespawner.ResetAsync(ConnectionString); // in case the test database already exists and is populated
    }

    [OneTimeTearDown]
    public static async Task OneTimeTearDown()
    {
        var dbContext = DbContextFactory.Create();
        await dbContext.Database.EnsureDeletedAsync();
    }


    [SetUp]
    public virtual async Task SetUp()
    {
        this.ArrangeAssertDbContext = DbContextFactory.Create();
        this.ArrangeAssertDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking; // ensures the reads return the values from the database and NOT from memory

        await Task.CompletedTask;
    }

    [TearDown]
    public virtual async Task TearDown()
    {
        await DbRespawner.ResetAsync(ConnectionString);
    }
}
