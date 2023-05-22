using Infrastructure.Tests.Integration.DataAccess.Common;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Respawn;

using Xunit;

namespace Infrastructure.Tests.Integration.AbstractBases;

public class DatabaseFixture : IAsyncLifetime
{
    public string ConnectionString { get; private set; } = default!;
    public FuturesTradingDbContextFactory DbContextFactory { get; private set; } = default!;
    
    private Respawner DbRespawner = default!;
    public async Task ClearDatabaseAsync() => await this.DbRespawner.ResetAsync(this.ConnectionString);


    public async Task InitializeAsync()
    {
        var configuration = new ConfigurationManager();
        configuration.AddJsonFile("testsettings.json", optional: false);
        this.ConnectionString = configuration.GetConnectionString("TradingHistoryDB")!.Replace("Initial Catalog=TradingHistoryDB-TestDatabase;", $"Initial Catalog=TradingHistoryDB-TestDatabase-{Guid.NewGuid()};");
        
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
