using Application.Interfaces.Services.Bybit;

using Bogus;

using Domain.Models.Common;

using Infrastructure.Extensions;
using Infrastructure.Services.Bybit;
using Infrastructure.Services.General;
using Infrastructure.Tests.Integration.Bybit.Abstract;
using Infrastructure.Tests.Integration.Common.Fixtures;

using MediatR;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
    protected readonly IBybitUsdFuturesTradingApiClient TradingClient;
    protected readonly IBybitUsdFuturesMarketDataProvider MarketDataProvider;
    protected readonly IBybitFuturesAccountDataProvider FuturesAccount;

    protected readonly Func<Task> ClearDatabaseAsyncFunc;

    public BybitUsdFuturesTradingServiceTestsBase(DatabaseFixture databaseFixture) : base()
    {
        var serviceProvider = this.BuildServiceProvider(databaseFixture);

        this.FuturesAccount = new BybitFuturesAccountDataProvider(this.BybitClient.UsdPerpetualApi.Account);
        this.TradingClient = new BybitUsdFuturesTradingApiClient(this.BybitClient.UsdPerpetualApi.Trading);
        this.MarketDataProvider = new BybitUsdFuturesMarketDataProvider(new DateTimeProvider(), this.BybitClient.UsdPerpetualApi.ExchangeData);

        var mediator = serviceProvider.GetRequiredService<IMediator>();
        this.SUT = new BybitUsdFuturesTradingService(this.CurrencyPair, this.Leverage, this.FuturesAccount, this.MarketDataProvider, this.TradingClient, mediator);


        this.ClearDatabaseAsyncFunc = databaseFixture.ClearDatabaseAsync;
    }
    private ServiceProvider BuildServiceProvider(DatabaseFixture databaseFixture)
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationManager();
        configuration.AddJsonFile("testsettings.json", optional: false);
        configuration["ConnectionStrings:TradingHistoryDB"] = databaseFixture.ConnectionString;

        services.AddServices(configuration);
        return services.BuildServiceProvider();
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
