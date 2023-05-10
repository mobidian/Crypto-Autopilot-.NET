using Application.Data.Mapping;

using Bybit.Net.Enums;

using Domain.Models;

using Infrastructure.Database.Contexts;
using Infrastructure.Tests.Integration.DataAccess.Common;
using Infrastructure.Tests.Integration.DataAccess.Extensions;

using Respawn;

namespace Infrastructure.Tests.Integration.DataAccess.Abstract;

public abstract class FuturesRepositoriesTestsBase
{
    protected const string ConnectionString = """Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TradingHistoryDB-TestDatabase;Integrated Security=True;Connect Timeout=60;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False""";
    protected static FuturesTradingDbContextFactory DbContextFactory = new(ConnectionString);
    protected static Respawner DbRespawner = default!;

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

    [TearDown]
    public virtual async Task TearDown()
    {
        await DbRespawner.ResetAsync(ConnectionString);
    }


    #region Fakers
    private const int decimals = 4;


    protected readonly Faker Faker = new Faker();

    protected readonly Faker<CurrencyPair> CurrencyPairGenerator = new Faker<CurrencyPair>()
        .CustomInstantiator(f => new CurrencyPair(f.Finance.Currency().Code, f.Finance.Currency().Code));

    protected readonly Faker<FuturesPosition> FuturesPositionsGenerator = new Faker<FuturesPosition>()
        .RuleFor(p => p.CryptoAutopilotId, f => Guid.NewGuid())
        .RuleFor(p => p.CurrencyPair, f => new CurrencyPair("BTC", "USDT"))
        .RuleFor(p => p.Margin, f => f.Random.Decimal(1, 1000, decimals))
        .RuleFor(p => p.Leverage, f => f.Random.Decimal(1, 100, decimals))
        .RuleFor(p => p.EntryPrice, f => f.Random.Decimal(5000, 15000, decimals))
        .RuleFor(p => p.Quantity, (_, p) => Math.Round(p.Margin * p.Leverage / p.EntryPrice, decimals))
        .RuleSet(PositionSide.Buy.ToRuleSetName(), set =>
        {
            set.RuleFor(p => p.Side, PositionSide.Buy);
            set.RuleFor(p => p.ExitPrice, (f, p) => f.Random.Decimal(p.EntryPrice, p.EntryPrice + 3000, decimals));
        })
        .RuleSet(PositionSide.Sell.ToRuleSetName(), set =>
        {
            set.RuleFor(p => p.Side, PositionSide.Sell);
            set.RuleFor(p => p.ExitPrice, (f, p) => f.Random.Decimal(p.EntryPrice, p.EntryPrice - 3000, decimals));
        });

    protected readonly Faker<FuturesOrder> FuturesOrdersGenerator = new Faker<FuturesOrder>()
        .RuleFor(o => o.BybitID, f => Guid.NewGuid())
        .RuleFor(o => o.CurrencyPair, f => new CurrencyPair("BTC", "USDT"))
        .RuleFor(o => o.CreateTime, f => f.Date.Past(1))
        .RuleFor(o => o.UpdateTime, (f, p) => p.CreateTime.AddHours(f.Random.Int(0, 12)))
        .RuleFor(o => o.Price, f => f.Random.Decimal(5000, 15000, decimals))
        .RuleFor(o => o.Quantity, f => f.Random.Decimal(100, 300, decimals))
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
            set.RuleFor(o => o.StopLoss, (f, p) => f.Random.Decimal(p.Price, p.Price - 3000, decimals));
            set.RuleFor(o => o.TakeProfit, (f, p) => f.Random.Decimal(p.Price, p.Price + 3000, decimals));
        })
        .RuleSet(OrderSide.Sell.ToRuleSetName(), set =>
        {
            set.RuleFor(o => o.Side, f => OrderSide.Sell);
            set.RuleFor(o => o.StopLoss, (f, p) => f.Random.Decimal(p.Price, p.Price + 3000, decimals));
            set.RuleFor(o => o.TakeProfit, (f, p) => f.Random.Decimal(p.Price, p.Price - 3000, decimals));
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


    protected FuturesTradingDbContext ArrangeAssertDbContext = default!;
    protected async Task InsertRelatedPositionAndOrdersAsync(FuturesPosition position, IEnumerable<FuturesOrder> orders)
    {
        using var transaction = await this.ArrangeAssertDbContext.Database.BeginTransactionAsync();


        var positionDbEntity = position.ToDbEntity();
        await this.ArrangeAssertDbContext.FuturesPositions.AddAsync(positionDbEntity);
        await this.ArrangeAssertDbContext.SaveChangesAsync();

        var futuresOrderDbEntities = orders.Select(x =>
        {
            var entity = x.ToDbEntity();
            entity.PositionId = positionDbEntity.Id;
            return entity;
        });
        await this.ArrangeAssertDbContext.FuturesOrders.AddRangeAsync(futuresOrderDbEntities);
        await this.ArrangeAssertDbContext.SaveChangesAsync();


        await transaction.CommitAsync();
    }
}
