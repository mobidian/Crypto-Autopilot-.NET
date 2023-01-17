using Application.Data.Entities.Common;
using Application.Interfaces.Services.General;

using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;

using Domain.Models;

using Infrastructure.Database.Contexts;
using Infrastructure.Services.Trading;
using Infrastructure.Tests.Integration.Common;

using Respawn;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Common;

public abstract class FuturesTradesDBServiceTestsBase
{
    protected const string ConnectionString = Credentials.TestDbConnectionString;

    protected FuturesTradesDBService SUT;
    protected FuturesTradingDbContext dbContext;
    protected IDateTimeProvider DateTimeProvider = Substitute.For<IDateTimeProvider>();
    
    protected readonly Faker<Candlestick> CandlestickGenerator = new Faker<Candlestick>()
        .RuleFor(x => x.CurrencyPair, f => new CurrencyPair(f.Finance.Currency().Code, f.Finance.Currency().Code))
        .RuleFor(c => c.Date, f => f.Date.Recent(365))
        .RuleFor(c => c.Open, f => f.Random.Decimal(1000, 1500))
        .RuleFor(c => c.High, (f, c) => f.Random.Decimal(c.Open, c.Open + 100))
        .RuleFor(c => c.Low, (f, c) => f.Random.Decimal(c.Open - 100, c.Open))
        .RuleFor(c => c.Close, (f, c) => f.Random.Decimal(1000, 1500))
        .RuleFor(c => c.Volume, f => f.Random.Decimal(100000, 300000));

    protected readonly Faker<BinanceFuturesOrder> FuturesOrderGenerator = new Faker<BinanceFuturesOrder>()
        .RuleFor(o => o.Id, f => f.Random.Long())
        .RuleFor(o => o.CreateTime, f => f.Date.Recent(365))
        .RuleFor(o => o.Side, f => f.Random.Enum<OrderSide>())
        .RuleFor(o => o.Type, f => f.Random.Enum<FuturesOrderType>())
        .RuleFor(o => o.Price, f => f.Random.Decimal(0, 1000))
        .RuleFor(o => o.Quantity, f => f.Random.Decimal(0, 10));


    private Respawner DbRespawner;
    protected async Task ClearDatabaseAsync() => await this.DbRespawner.ResetAsync(ConnectionString);



    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        this.ConfigureIDateTiemProviderMock();

        this.dbContext = new FuturesTradingDbContext(ConnectionString, this.DateTimeProvider);
        await this.dbContext.Database.EnsureCreatedAsync();

        this.SUT = new FuturesTradesDBService(this.dbContext);

        this.DbRespawner = await Respawner.CreateAsync(ConnectionString, new RespawnerOptions { CheckTemporalTables = true });
        await this.ClearDatabaseAsync(); // in case the test database already exists and is populated
    }
    private void ConfigureIDateTiemProviderMock()
    {
        this.DateTimeProvider.Now.Returns(new DateTime(2023, 01, 01, 04, 20, 0));
        this.DateTimeProvider.UtcNow.Returns(new DateTime(2023, 01, 01, 02, 20, 0));
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await this.dbContext.Database.EnsureDeletedAsync();
    }


    protected void AssertAgainstAddedEntityAuditRecords(BaseEntity addedEntity)
    {
        addedEntity.RecordCreatedDate.Should().Be(this.DateTimeProvider.Now);
        addedEntity.RecordModifiedDate.Should().Be(DateTime.MinValue);
    }
}
