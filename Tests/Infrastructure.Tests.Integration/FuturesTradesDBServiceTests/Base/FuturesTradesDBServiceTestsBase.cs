using Application.Data.Mapping;

using Bybit.Net.Enums;

using Domain.Models;

using Infrastructure.Database.Contexts;
using Infrastructure.Services;
using Infrastructure.Tests.Integration.Common;
using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Common;
using Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Extensions;

using Respawn;

namespace Infrastructure.Tests.Integration.FuturesTradesDBServiceTests.Base;

public abstract class FuturesTradesDBServiceTestsBase
{
    private static readonly string ConnectionString = new SecretsManager().GetConnectionString("TradingHistoryDB-TestDatabase");
    private static FuturesTradingDbContextFactory DbContextFactory = default!;
    private static Respawner DbRespawner;


    protected FuturesTradesDBService SUT;
    protected FuturesTradingDbContext DbContext;


    #region Fakers
    protected readonly Faker Faker = new Faker();

    protected readonly Faker<CurrencyPair> CurrencyPairGenerator = new Faker<CurrencyPair>()
        .CustomInstantiator(f => new CurrencyPair(f.Finance.Currency().Code, f.Finance.Currency().Code));

    protected readonly Faker<FuturesPosition> FuturesPositionsGenerator = new Faker<FuturesPosition>()
        .RuleFor(p => p.CryptoAutopilotId, f => Guid.NewGuid())
        .RuleFor(p => p.CurrencyPair, f => new CurrencyPair("BTC", "USDT"))
        .RuleFor(p => p.Margin, f => f.Random.Decimal(1, 1000))
        .RuleFor(p => p.Leverage, f => f.Random.Decimal(1, 100))
        .RuleFor(p => p.EntryPrice, f => f.Random.Decimal(5000, 15000))
        .RuleSet(PositionSide.Buy.ToRuleSetName(), set =>
        {
            set.RuleFor(p => p.Side, PositionSide.Buy);
            set.RuleFor(p => p.Quantity, (_, p) => p.Margin * p.Leverage / p.EntryPrice);
            set.RuleFor(p => p.ExitPrice, (f, p) => f.Random.Decimal(p.EntryPrice, p.EntryPrice + 3000));
        })
        .RuleSet(PositionSide.Sell.ToRuleSetName(), set =>
        {
            set.RuleFor(p => p.Side, PositionSide.Sell);
            set.RuleFor(p => p.Quantity, (_, p) => p.Margin * p.Leverage / p.EntryPrice);
            set.RuleFor(p => p.ExitPrice, (f, p) => f.Random.Decimal(p.EntryPrice, p.EntryPrice - 3000));
        });

    protected readonly Faker<FuturesOrder> FuturesOrdersGenerator = new Faker<FuturesOrder>()
        .RuleFor(o => o.BybitID, f => Guid.NewGuid())
        .RuleFor(o => o.CurrencyPair, f => new CurrencyPair("BTC", "USDT"))
        .RuleFor(o => o.CreateTime, f => f.Date.Past(1))
        .RuleFor(o => o.UpdateTime, (f, p) => p.CreateTime.AddHours(f.Random.Int(0, 12)))
        .RuleFor(o => o.Price, f => f.Random.Decimal(5000, 15000))
        .RuleFor(o => o.Quantity, f => f.Random.Decimal(100, 300))
        .RuleFor(o => o.Status, OrderStatus.Created)
        .RuleSet(OrderType.Limit.ToRuleSetName(), set =>
        {
            set.RuleFor(o => o.Type, OrderType.Limit);
            set.RuleFor(o => o.TimeInForce, TimeInForce.GoodTillCanceled);
        })
        .RuleSet(OrderType.Market.ToRuleSetName(), set =>
        {
            set.RuleFor(o => o.Type, OrderType.Market);
            set.RuleFor(o => o.TimeInForce, TimeInForce.ImmediateOrCancel);
        })
        .RuleSet(OrderStatus.Filled.ToRuleSetName(), set => set.RuleFor(o => o.Status, OrderStatus.Filled))
        .RuleSet(OrderSide.Buy.ToRuleSetName(), set =>
        {
            set.RuleFor(o => o.Side, f => OrderSide.Buy);
            set.RuleFor(o => o.StopLoss, (f, p) => f.Random.Decimal(p.Price, p.Price - 3000));
            set.RuleFor(o => o.TakeProfit, (f, p) => f.Random.Decimal(p.Price, p.Price + 3000));
        })
        .RuleSet(OrderSide.Sell.ToRuleSetName(), set =>
        {
            set.RuleFor(o => o.Side, f => OrderSide.Sell);
            set.RuleFor(o => o.StopLoss, (f, p) => f.Random.Decimal(p.Price, p.Price + 3000));
            set.RuleFor(o => o.TakeProfit, (f, p) => f.Random.Decimal(p.Price, p.Price - 3000));
        })
        .RuleSet(PositionSide.Buy.ToRuleSetName(), set =>
        {
            set.RuleFor(o => o.PositionSide, f => PositionSide.Buy);
        })
        .RuleSet(PositionSide.Sell.ToRuleSetName(), set =>
        {
            set.RuleFor(o => o.PositionSide, f => PositionSide.Sell);
        });
    #endregion


    [OneTimeSetUp]
    public static async Task OneTimeSetUp()
    {
        DbContextFactory = new FuturesTradingDbContextFactory(ConnectionString);

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
        this.DbContext = DbContextFactory.Create();
        this.SUT = new FuturesTradesDBService(this.DbContext);

        await Task.CompletedTask;
    }

    [TearDown]
    public virtual async Task TearDown()
    {
        await DbRespawner.ResetAsync(ConnectionString);
    }
    

    protected async Task InsertRelatedPositionAndOrdersAsync(FuturesPosition position, IEnumerable<FuturesOrder> orders)
    {
        using var transaction = await this.DbContext.Database.BeginTransactionAsync();


        var positionDbEntity = position.ToDbEntity();
        await this.DbContext.FuturesPositions.AddAsync(positionDbEntity);
        await this.DbContext.SaveChangesAsync();
        
        var futuresOrderDbEntities = orders.Select(x =>
        {
            var entity = x.ToDbEntity();
            entity.PositionId = positionDbEntity.Id;
            return entity;
        });
        await this.DbContext.FuturesOrders.AddRangeAsync(futuresOrderDbEntities);
        await this.DbContext.SaveChangesAsync();


        await transaction.CommitAsync();
    }
}
