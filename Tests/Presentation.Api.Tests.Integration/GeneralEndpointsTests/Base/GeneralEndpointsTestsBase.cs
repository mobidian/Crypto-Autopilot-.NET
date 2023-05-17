using System.Reflection;

using Application.Interfaces.Services;
using Application.Interfaces.Services.DataAccess;

using Bybit.Net.Enums;

using CryptoAutopilot.Api.Services.Interfaces;

using Domain.Models.Common;
using Domain.Models.Orders;

using Infrastructure;
using Infrastructure.Database;
using Infrastructure.Services.DataAccess;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Presentation.Api.Tests.Integration.Common;

using Respawn;

namespace Presentation.Api.Tests.Integration.GeneralEndpointsTests.Base;

public abstract class GeneralEndpointsTestsBase
{
    protected const string ConnectionString = """Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TradingHistoryDB-TestDatabase;Integrated Security=True;Connect Timeout=60;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False""";


    #region Fakers
    private const int precision = 4;

    protected readonly Faker<CurrencyPair> CurrencyPairGenerator = new Faker<CurrencyPair>()
        .CustomInstantiator(f => new CurrencyPair(f.Finance.Currency().Code, f.Finance.Currency().Code));

    protected readonly Faker<Candlestick> CandlestickGenerator = new Faker<Candlestick>()
        .RuleFor(c => c.CurrencyPair, f => new CurrencyPair(f.Finance.Currency().Code, f.Finance.Currency().Code))
        .RuleFor(c => c.Date, f => f.Date.Recent(365))
        .RuleFor(c => c.Open, f => Math.Round(f.Random.Decimal(1000, 1500), precision))
        .RuleFor(c => c.High, (f, c) => Math.Round(f.Random.Decimal(c.Open, c.Open + 100), precision))
        .RuleFor(c => c.Low, (f, c) => Math.Round(f.Random.Decimal(c.Open - 100, c.Open), precision))
        .RuleFor(c => c.Close, (f, c) => Math.Round(f.Random.Decimal(1000, 1500), precision))
        .RuleFor(c => c.Volume, f => Math.Round(f.Random.Decimal(100000, 300000), precision));

    protected readonly Faker<FuturesOrder> FuturesOrderGenerator = new Faker<FuturesOrder>()
        .RuleFor(o => o.BybitID, f => Guid.NewGuid())
        .RuleFor(o => o.CreateTime, f => f.Date.Recent(365))
        .RuleFor(o => o.UpdateTime, f => f.Date.Recent(365))
        .RuleFor(o => o.Side, f => f.Random.Enum<OrderSide>())
        .RuleFor(o => o.PositionSide, f => f.Random.Enum<PositionSide>())
        .RuleFor(o => o.Type, f => f.Random.Enum<OrderType>())
        .RuleFor(o => o.Price, f => Math.Round(f.Random.Decimal(0, 1000), precision))
        .RuleFor(o => o.Quantity, f => Math.Round(f.Random.Decimal(0, 10), precision))
        .RuleFor(o => o.StopLoss, f => Math.Round(f.Random.Decimal(0, 1000), precision))
        .RuleFor(o => o.TakeProfit, f => Math.Round(f.Random.Decimal(0, 1000), precision))
        .RuleFor(o => o.TimeInForce, f => f.Random.Enum<TimeInForce>())
        .RuleFor(o => o.Status, f => f.Random.Enum<OrderStatus>());

    protected readonly Faker<IStrategyEngine> StrategyEnginesGenerator = new Faker<IStrategyEngine>()
        .CustomInstantiator(f =>
        {
            var types = typeof(IInfrastructureMarker).Assembly.DefinedTypes.Where(typeInfo => !typeInfo.IsInterface && !typeInfo.IsAbstract && typeof(IStrategyEngine).IsAssignableFrom(typeInfo));
            var type = f.PickRandom(types);

            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var parameters = new object[] { Guid.NewGuid(), new CurrencyPair(f.Finance.Currency().Code, f.Finance.Currency().Code), f.PickRandom<KlineInterval>() };
            return (IStrategyEngine)Activator.CreateInstance(type, flags, null, parameters, null)!;
        }); 
    #endregion


    protected readonly ApiFactory ApiFactory;
    protected readonly HttpClient HttpClient;

    private FuturesTradingDbContext DbContext;
    protected readonly IFuturesOrdersRepository OrdersRepository;
    protected readonly IStrategiesTracker StrategiesTracker;
    
    public GeneralEndpointsTestsBase()
    {
        this.ApiFactory = new ApiFactory();
        this.HttpClient = this.ApiFactory.CreateClient();

        var optionsBuilder = new DbContextOptionsBuilder();
        optionsBuilder.UseSqlServer(ConnectionString);
        this.DbContext = new FuturesTradingDbContext(optionsBuilder.Options);
        this.OrdersRepository = new FuturesOrdersRepository(this.DbContext);

        this.StrategiesTracker = this.ApiFactory.Services.GetRequiredService<IStrategiesTracker>();
    }

    

    private Respawner DbRespawner;
    protected async Task ClearDatabaseAsync() => await this.DbRespawner.ResetAsync(ConnectionString);



    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        await this.DbContext.Database.EnsureCreatedAsync();

        this.DbRespawner = await Respawner.CreateAsync(ConnectionString, new RespawnerOptions { CheckTemporalTables = true });
        await this.ClearDatabaseAsync(); // in case the test database already exists and is populated
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await this.DbContext.Database.EnsureDeletedAsync();
    }


    [TearDown]
    public async Task TearDown()
    {
        await this.ClearDatabaseAsync();
        this.StrategiesTracker.Clear();
    }

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
}
