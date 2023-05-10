using Application.Interfaces.Services.Bybit;
using Application.Interfaces.Services.DataAccess;

using Domain.Models;

using Infrastructure.Database.Contexts;
using Infrastructure.Services.Bybit;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.General;
using Infrastructure.Tests.Integration.Bybit.Abstract;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Respawn;

namespace Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.AbstractBase;

public abstract class BybitUsdFuturesTradingServiceTestsBase : BybitServicesTestBase
{
    protected readonly CurrencyPair CurrencyPair = new CurrencyPair("BTC", "USDT");
    protected readonly Faker faker = new Faker();

    protected decimal Margin = 100;
    protected readonly decimal Leverage = 10m;

    protected readonly IBybitUsdFuturesTradingService SUT;
    protected readonly IBybitUsdFuturesTradingApiClient TradingClient;
    protected readonly IBybitUsdFuturesMarketDataProvider MarketDataProvider;
    protected readonly IBybitFuturesAccountDataProvider FuturesAccount;
    
    private readonly FuturesTradingDbContext DbContext;
    private Respawner DbRespawner = default!;
    
    public BybitUsdFuturesTradingServiceTestsBase() : base()
    {
        var services = new ServiceCollection();
        services.AddDbContext<FuturesTradingDbContext>(options => options.UseSqlServer(ConnectionString));
        services.AddScoped<IFuturesOrdersRepository, FuturesOrdersRepository>();
        services.AddScoped<IFuturesPositionsRepository, FuturesPositionsRepository>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IInfrastructureMarker>());
        var serviceProvider = services.BuildServiceProvider();

        this.FuturesAccount = new BybitFuturesAccountDataProvider(this.BybitClient.UsdPerpetualApi.Account);
        this.TradingClient = new BybitUsdFuturesTradingApiClient(this.BybitClient.UsdPerpetualApi.Trading);
        this.MarketDataProvider = new BybitUsdFuturesMarketDataProvider(new DateTimeProvider(), this.BybitClient.UsdPerpetualApi.ExchangeData);

        var mediator = serviceProvider.GetRequiredService<IMediator>();    
        this.SUT = new BybitUsdFuturesTradingService(this.CurrencyPair, this.Leverage, this.FuturesAccount, this.MarketDataProvider, this.TradingClient, mediator);


        this.DbContext = serviceProvider.GetRequiredService<FuturesTradingDbContext>();
    }


    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        await this.DbContext.Database.EnsureCreatedAsync();
        
        var connectionString = this.DbContext.Database.GetConnectionString()!;
        this.DbRespawner = await Respawner.CreateAsync(connectionString, new RespawnerOptions { CheckTemporalTables = true });
        await this.DbRespawner.ResetAsync(connectionString); // in case the test database already exists and is populated
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await this.DbContext.Database.EnsureDeletedAsync();
    }


    [TearDown]
    public async Task TearDown()
    {
        await Task.WhenAll(this.SUT.CloseAllPositionsAsync(),
                           this.SUT.CancelAllLimitOrdersAsync());
        
        await this.DbRespawner.ResetAsync(this.DbContext.Database.GetConnectionString()!);
    }
}
