using Bybit.Net.Enums;

using Domain.Models;

using Infrastructure.Database.Contexts;
using Infrastructure.Services;
using Infrastructure.Tests.Integration.Common;

using Microsoft.EntityFrameworkCore;

using Respawn;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;

public abstract class FuturesTradesDBServiceTestsBase
{
    protected readonly string ConnectionString = new SecretsManager().GetConnectionString("TradingHistoryDB-TestDatabase");

    protected FuturesTradesDBService SUT;
    protected FuturesTradingDbContext DbContext;

    
    protected readonly Random Random = new Random();

    protected readonly Faker<CurrencyPair> CurrencyPairGenerator = new Faker<CurrencyPair>()
        .CustomInstantiator(f => new CurrencyPair(f.Finance.Currency().Code, f.Finance.Currency().Code));
    
    protected readonly Faker<FuturesOrder> FuturesOrderGenerator = new Faker<FuturesOrder>()
        .RuleFor(o => o.BybitID, f => Guid.NewGuid())
        .RuleFor(o => o.CurrencyPair, f => new CurrencyPair(f.Finance.Currency().Code, f.Finance.Currency().Code))
        .RuleFor(o => o.CreateTime, f => f.Date.Recent(365))
        .RuleFor(o => o.UpdateTime, f => f.Date.Recent(365))
        .RuleFor(o => o.Side, f => f.Random.Enum<OrderSide>())
        .RuleFor(o => o.PositionSide, f => f.Random.Enum<PositionSide>())
        .RuleFor(o => o.Type, f => f.Random.Enum<OrderType>())
        .RuleFor(o => o.Price, f => f.Random.Decimal(0, 1000))
        .RuleFor(o => o.Quantity, f => f.Random.Decimal(0, 10))
        .RuleFor(o => o.StopLoss, f => f.Random.Decimal(0, 1000))
        .RuleFor(o => o.TakeProfit, f => f.Random.Decimal(0, 1000))
        .RuleFor(o => o.TimeInForce, f => f.Random.Enum<TimeInForce>())
        .RuleFor(o => o.Status, f => f.Random.Enum<OrderStatus>());
    
    
    private Respawner DbRespawner;
    protected async Task ClearDatabaseAsync() => await this.DbRespawner.ResetAsync(this.ConnectionString);



    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        var optionsBuilder = new DbContextOptionsBuilder();
        optionsBuilder.UseSqlServer(this.ConnectionString);
        this.DbContext = new FuturesTradingDbContext(optionsBuilder.Options);
        
        await this.DbContext.Database.EnsureCreatedAsync();

        this.SUT = new FuturesTradesDBService(this.DbContext);

        this.DbRespawner = await Respawner.CreateAsync(this.ConnectionString, new RespawnerOptions { CheckTemporalTables = true });
        await this.ClearDatabaseAsync(); // in case the test database already exists and is populated
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await this.DbContext.Database.EnsureDeletedAsync();
    }

    
    [TearDown]
    public virtual async Task TearDown() => await this.ClearDatabaseAsync();
}
