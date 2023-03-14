using Application.Interfaces.Services;
using Application.Interfaces.Services.Bybit;

using Bybit.Net.Clients;
using Bybit.Net.Enums;
using Bybit.Net.Objects;

using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;

using Domain.Models;

using Infrastructure.Database.Contexts;
using Infrastructure.Services;
using Infrastructure.Services.Bybit;
using Infrastructure.Services.General;
using Infrastructure.Tests.Integration.Common;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Respawn;

namespace Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.AbstractBase;

public abstract class BybitUsdFuturesTradingServiceTestsBase
{
    private readonly SecretsManager SecretsManager = new SecretsManager();

    protected readonly Faker faker = new Faker();

    protected readonly BybitUsdFuturesTradingService SUT;
    protected readonly CurrencyPair CurrencyPair = new CurrencyPair("BTC", "USDT");
    protected decimal Margin = 100;
    protected readonly decimal Leverage = 10m;
    protected readonly IBybitFuturesAccountDataProvider FuturesAccount;
    protected readonly IBybitUsdFuturesMarketDataProvider MarketDataProvider;
    protected readonly IBybitUsdFuturesTradingApiClient TradingClient;
    protected readonly IMediator Mediator;
    
    private readonly IServiceProvider Services;
    private Respawner DbRespawner = default!;
    
    public BybitUsdFuturesTradingServiceTestsBase()
    {
        var services = new ServiceCollection();
        services.AddDbContext<FuturesTradingDbContext>(options => options.UseSqlServer(this.SecretsManager.GetConnectionString("TradingHistoryDB-TestDatabase")!));
        services.AddSingleton<IFuturesTradesDBService, FuturesTradesDBService>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IInfrastructureMarker>());
        this.Services = services.BuildServiceProvider();

        
        var bybitClient = new BybitClient(new BybitClientOptions
        {
            UsdPerpetualApiOptions = new RestApiClientOptions
            {
                ApiCredentials = new ApiCredentials(this.SecretsManager.GetSecret("BybitTestnetApiCredentials:key"), this.SecretsManager.GetSecret("BybitTestnetApiCredentials:secret")),
                BaseAddress = "https://api-testnet.bybit.com"
            }
        });

        this.FuturesAccount = new BybitFuturesAccountDataProvider(bybitClient.UsdPerpetualApi.Account);
        this.TradingClient = new BybitUsdFuturesTradingApiClient(bybitClient.UsdPerpetualApi.Trading);
        this.MarketDataProvider = new BybitUsdFuturesMarketDataProvider(new DateTimeProvider(), bybitClient.UsdPerpetualApi.ExchangeData);

        this.Mediator = this.Services.GetRequiredService<IMediator>();
        
        this.SUT = new BybitUsdFuturesTradingService(this.CurrencyPair, this.Leverage, this.FuturesAccount, this.MarketDataProvider, this.TradingClient, this.Mediator);
    }


    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        var dbContext = this.Services.GetRequiredService<FuturesTradingDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        
        this.DbRespawner = await Respawner.CreateAsync(this.SecretsManager.GetConnectionString("TradingHistoryDB-TestDatabase")!, new RespawnerOptions { CheckTemporalTables = true });
        await this.DbRespawner.ResetAsync(dbContext.Database.GetConnectionString()!); // in case the test database already exists and is populated
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        var dbContext = this.Services.GetRequiredService<FuturesTradingDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
    }


    [TearDown]
    public async Task TearDown()
    {
        if (this.SUT.LongPosition is not null)
            await this.SUT.ClosePositionAsync(PositionSide.Buy);

        if (this.SUT.ShortPosition is not null)
            await this.SUT.ClosePositionAsync(PositionSide.Sell);

        if (this.SUT.BuyLimitOrder is not null)
            await this.SUT.CancelLimitOrderAsync(OrderSide.Buy);

        if (this.SUT.SellLimitOrder is not null)
            await this.SUT.CancelLimitOrderAsync(OrderSide.Sell);


        var dbContext = this.Services.GetRequiredService<FuturesTradingDbContext>();
        await this.DbRespawner.ResetAsync(dbContext.Database.GetConnectionString()!);
    }
}
