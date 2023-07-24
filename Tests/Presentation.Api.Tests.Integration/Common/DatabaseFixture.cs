using Microsoft.EntityFrameworkCore;

using Respawn;

using Xunit;

namespace Presentation.Api.Tests.Integration.Common;

[CollectionDefinition(nameof(DatabaseFixture))]
public class DatabaseCollectionFixture : ICollectionFixture<DatabaseFixture>
{
}

public class DatabaseFixture : IAsyncLifetime
{
    public string ConnectionString { get; } = $"""Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TradingHistoryDB-TestDatabase-{Guid.NewGuid()};Integrated Security=True;Connect Timeout=60;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False""";
    public FuturesTradingDbContextFactory DbContextFactory { get; private set; } = default!;

    private Respawner DbRespawner = default!;
    public async Task ClearDatabaseAsync() => await this.DbRespawner.ResetAsync(this.ConnectionString);


    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder().UseSqlServer(this.ConnectionString).Options;
        this.DbContextFactory = new FuturesTradingDbContextFactory(options);

        var ctx = this.DbContextFactory.Create();
        await ctx.Database.EnsureCreatedAsync();

        this.DbRespawner = await Respawner.CreateAsync(this.ConnectionString, new RespawnerOptions
        {
            CheckTemporalTables = true
        });
        await this.ClearDatabaseAsync(); // in case the test database already exists and is populated
    }

    public async Task DisposeAsync()
    {
        var dbContext = this.DbContextFactory.Create();
        await dbContext.Database.EnsureDeletedAsync();
    }
}