using Application.Interfaces.Services.Trading;

using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;

using Domain.Models;

using Infrastructure.Database.Contexts;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Presentation.Api.Tests.Integration.Common;

using Respawn;

namespace Presentation.Api.Tests.Integration.GeneralEndpointsTests.Base;

public abstract class GeneralEndpointsTestsBase
{
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
        .RuleFor(o => o.Type, f => f.Random.Enum<FuturesOrderType>())
        .RuleFor(o => o.WorkingType, f => f.Random.Enum<WorkingType>())
        .RuleFor(o => o.Price, f => f.Random.Decimal(0, 1000))
        .RuleFor(o => o.AvgPrice, f => f.Random.Decimal(0, 1000))
        .RuleFor(o => o.StopPrice, f => f.Random.Decimal(0, 1000))
        .RuleFor(o => o.Quantity, f => f.Random.Decimal(0, 10))
        .RuleFor(o => o.PriceProtect, f => f.PickRandom(true, false))
        .RuleFor(o => o.TimeInForce, f => f.Random.Enum<TimeInForce>())
        .RuleFor(o => o.Status, f => f.Random.Enum<OrderStatus>());



    protected readonly ApiFactory ApiFactory;
    protected readonly HttpClient HttpClient;

    protected readonly IFuturesTradesDBService FuturesTradesDBService;
    private FuturesTradingDbContext DbContext;

    public GeneralEndpointsTestsBase()
    {
        this.ApiFactory = new ApiFactory();
        this.HttpClient = this.ApiFactory.CreateClient();

        this.FuturesTradesDBService = this.ApiFactory.Services.GetRequiredService<IFuturesTradesDBService>();
        this.DbContext = this.ApiFactory.Services.GetRequiredService<FuturesTradingDbContext>();
    }


    
    private Respawner DbRespawner;
    protected async Task ClearDatabaseAsync() => await this.DbRespawner.ResetAsync(this.DbContext.Database.GetConnectionString()!);


    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        await this.DbContext.Database.EnsureCreatedAsync();

        this.DbRespawner = await Respawner.CreateAsync(this.DbContext.Database.GetConnectionString()!, new RespawnerOptions { CheckTemporalTables = true });
        await this.ClearDatabaseAsync(); // in case the test database already exists and is populated
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await this.DbContext.Database.EnsureDeletedAsync();
    }


    [TearDown]
    public async Task TearDown() => await this.ClearDatabaseAsync();
}
