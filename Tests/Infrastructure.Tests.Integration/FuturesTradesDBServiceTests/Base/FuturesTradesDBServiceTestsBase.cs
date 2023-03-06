using Application.Data.Entities.Common;
using Application.Interfaces.Services.General;
using Application.Mapping;

using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;

using Domain.Models;

using Infrastructure.Database.Contexts;
using Infrastructure.Services.Trading;
using Infrastructure.Tests.Integration.Common;

using Respawn;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;

public abstract class FuturesTradesDBServiceTestsBase
{
    protected readonly string ConnectionString = new SecretsManager().GetConnectionString("OrderHistoryDB");

    protected FuturesTradesDBService SUT;
    protected FuturesTradingDbContext DbContext;
    protected IDateTimeProvider DateTimeProvider = Substitute.For<IDateTimeProvider>();


    protected readonly Random Random = new Random();

    protected readonly Faker<CurrencyPair> CurrencyPairGenerator = new Faker<CurrencyPair>()
        .CustomInstantiator(f => new CurrencyPair(f.Finance.Currency().Code, f.Finance.Currency().Code));

    protected readonly Faker<Candlestick> CandlestickGenerator = new Faker<Candlestick>()
        .RuleFor(c => c.CurrencyPair, f => new CurrencyPair(f.Finance.Currency().Code, f.Finance.Currency().Code))
        .RuleFor(c => c.Date, f => f.Date.Recent(365))
        .RuleFor(c => c.Open, f => f.Random.Decimal(1000, 1500))
        .RuleFor(c => c.High, (f, c) => f.Random.Decimal(c.Open, c.Open + 100))
        .RuleFor(c => c.Low, (f, c) => f.Random.Decimal(c.Open - 100, c.Open))
        .RuleFor(c => c.Close, (f, c) => f.Random.Decimal(1000, 1500))
        .RuleFor(c => c.Volume, f => f.Random.Decimal(100000, 300000));

    protected readonly Faker<BinanceFuturesOrder> FuturesOrderGenerator = new Faker<BinanceFuturesOrder>()
        .RuleFor(o => o.Id, f => f.Random.Long(1000000))
        .RuleFor(o => o.CreateTime, f => f.Date.Recent(365))
        .RuleFor(o => o.UpdateTime, f => f.Date.Recent(365))
        .RuleFor(o => o.Side, f => f.Random.Enum<OrderSide>())
        .RuleFor(o => o.PositionSide, f => f.Random.Enum<PositionSide>())
        .RuleFor(o => o.Type, f => f.Random.Enum<FuturesOrderType>())
        .RuleFor(o => o.WorkingType, f => f.Random.Enum<WorkingType>())
        .RuleFor(o => o.Price, f => f.Random.Decimal(0, 1000))
        .RuleFor(o => o.AvgPrice, f => f.Random.Decimal(0, 1000))
        .RuleFor(o => o.StopPrice, f => f.Random.Decimal(0, 1000))
        .RuleFor(o => o.Quantity, f => f.Random.Decimal(0, 10))
        .RuleFor(o => o.PriceProtect, f => f.PickRandom(true, false))
        .RuleFor(o => o.TimeInForce, f => f.Random.Enum<TimeInForce>())
        .RuleFor(o => o.Status, f => f.Random.Enum<OrderStatus>());


    private Respawner DbRespawner;
    protected async Task ClearDatabaseAsync() => await this.DbRespawner.ResetAsync(this.ConnectionString);



    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        this.MockIDateTimeProvider();

        this.DbContext = new FuturesTradingDbContext(this.ConnectionString, this.DateTimeProvider);
        await this.DbContext.Database.EnsureCreatedAsync();

        this.SUT = new FuturesTradesDBService(this.DbContext);

        this.DbRespawner = await Respawner.CreateAsync(this.ConnectionString, new RespawnerOptions { CheckTemporalTables = true });
        await this.ClearDatabaseAsync(); // in case the test database already exists and is populated
    }
    private void MockIDateTimeProvider()
    {
        this.DateTimeProvider.Now.Returns(new DateTime(2023, 01, 01, 04, 20, 0));
        this.DateTimeProvider.UtcNow.Returns(new DateTime(2023, 01, 01, 02, 20, 0));
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await this.DbContext.Database.EnsureDeletedAsync();
    }


    [TearDown]
    public async Task TearDown() => await this.ClearDatabaseAsync();



    protected static CurrencyPair GetRandomCurrencyPairExcept(Faker f, CurrencyPair currencyPair)
    {
        CurrencyPair newCurrencyPair;
        do
        {
            newCurrencyPair = new CurrencyPair(f.Finance.Currency().Code, f.Finance.Currency().Code);
        }
        while (newCurrencyPair == currencyPair);

        return newCurrencyPair;
    }
    protected async Task InsertOneCandlestickAndMultipleFuturesOrdersAsync(Candlestick candlestick, List<BinanceFuturesOrder> futuresOrders)
    {
        using var transaction = await this.DbContext.Database.BeginTransactionAsync();

        var candlesticksEntity = candlestick.ToDbEntity();
        var futuresOrdersEntities = futuresOrders.Select(x => x.ToDbEntity()).ToList();

        await this.DbContext.Candlesticks.AddAsync(candlesticksEntity);
        await this.DbContext.SaveChangesAsync();

        futuresOrdersEntities.ForEach(x => x.CandlestickId = candlesticksEntity.Id);

        this.DbContext.FuturesOrders.AddRange(futuresOrdersEntities);
        await this.DbContext.SaveChangesAsync();

        await transaction.CommitAsync();
    }

    protected void AssertAgainstAddedEntitiesAuditRecords(IEnumerable<BaseEntity> addedEntities)
    {
        foreach (var addedEntity in addedEntities)
        {
            addedEntity.RecordCreatedDate.Should().Be(this.DateTimeProvider.Now);
            addedEntity.RecordModifiedDate.Should().Be(DateTime.MinValue);
        }
    }
}
