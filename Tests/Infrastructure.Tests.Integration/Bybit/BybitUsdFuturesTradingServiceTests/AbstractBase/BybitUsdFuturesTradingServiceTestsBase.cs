using Application.Interfaces.Services.Bybit;

using Bogus;

using Domain.Models.Common;

using Infrastructure.Extensions;
using Infrastructure.Factories;
using Infrastructure.Services.DataAccess.Database;
using Infrastructure.Tests.Integration.Bybit.Abstract;
using Infrastructure.Tests.Integration.Common.Fixtures;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Xunit;

namespace Infrastructure.Tests.Integration.Bybit.BybitUsdFuturesTradingServiceTests.AbstractBase;

[Collection(nameof(DatabaseFixture))]
public abstract class BybitUsdFuturesTradingServiceTestsBase : BybitServicesTestBase, IAsyncLifetime
{
    protected readonly CurrencyPair CurrencyPair = new CurrencyPair("BTC", "USDT");
    protected readonly Faker faker = new Faker();
    
    protected decimal Margin = 100;
    protected readonly decimal Leverage = 10m;

    protected readonly IBybitUsdFuturesTradingService SUT;
    protected readonly IBybitFuturesAccountDataProvider FuturesAccount;
    protected readonly IBybitUsdFuturesMarketDataProvider MarketDataProvider;
    protected readonly IBybitUsdFuturesTradingApiClient TradingClient;

    protected readonly Func<Task> ClearDatabaseAsyncFunc;
    
    public BybitUsdFuturesTradingServiceTestsBase(DatabaseFixture databaseFixture) : base()
    {
        this.Services.AddServices(this.Configuration);
        this.Services.RemoveAll<DbContextOptions>();
        this.Services.RemoveAll<DbContextOptions<FuturesTradingDbContext>>();
        this.Services.RemoveAll<FuturesTradingDbContext>();
        this.Services.AddDbContext<FuturesTradingDbContext>(options => options.UseSqlServer(databaseFixture.ConnectionString));

        this.Configuration["ConnectionStrings:TradingHistoryDB"] = databaseFixture.ConnectionString;
        this.Services.AddServices(this.Configuration);
        var serviceProvider = this.Services.BuildServiceProvider();
        
        this.SUT = serviceProvider.GetRequiredService<BybitUsdFuturesTradingServiceFactory>().Create(this.CurrencyPair, this.Leverage, serviceProvider);
        this.FuturesAccount = serviceProvider.GetRequiredService<IBybitFuturesAccountDataProvider>();
        this.MarketDataProvider = serviceProvider.GetRequiredService<IBybitUsdFuturesMarketDataProvider>();
        this.TradingClient = serviceProvider.GetRequiredService<IBybitUsdFuturesTradingApiClient>();
        

        this.ClearDatabaseAsyncFunc = databaseFixture.ClearDatabaseAsync;
    }
    

    public async Task InitializeAsync()
    {
        await Task.CompletedTask;
    }
    public async Task DisposeAsync()
    {
        await Task.WhenAll(this.SUT.CloseAllPositionsAsync(),
                           this.SUT.CancelAllLimitOrdersAsync());

        await this.ClearDatabaseAsyncFunc.Invoke();
    }
}
