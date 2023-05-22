using Infrastructure.Database;

using Microsoft.EntityFrameworkCore;

using Respawn;

using Xunit;

namespace Presentation.Api.Tests.Integration.Common;

public class DatabaseFixture : IAsyncLifetime
{
    private const string ConnectionString = """Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TradingHistoryDB-TestDatabase;Integrated Security=True;Connect Timeout=60;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False""";
    public FuturesTradingDbContext DbContext { get; private set; } = default!;

    private Respawner DbRespawner = default!;
    public async Task ClearDatabaseAsync() => await this.DbRespawner.ResetAsync(ConnectionString);



    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder().UseSqlServer(ConnectionString).Options;
        this.DbContext = new FuturesTradingDbContext(options);
        
        await this.DbContext.Database.EnsureCreatedAsync();
        
        this.DbRespawner = await Respawner.CreateAsync(ConnectionString, new RespawnerOptions { CheckTemporalTables = true });
        await this.ClearDatabaseAsync(); // in case the test database already exists and is populated
    }

    public async Task DisposeAsync()
    {
        await this.DbContext.Database.EnsureDeletedAsync();
    }
}


[CollectionDefinition(nameof(DatabaseFixture))]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
}