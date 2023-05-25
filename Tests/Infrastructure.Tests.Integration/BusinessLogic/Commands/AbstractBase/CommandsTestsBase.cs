using Infrastructure.Extensions;
using Infrastructure.Services.DataAccess.Database;
using Infrastructure.Tests.Integration.Common.Fakers;
using Infrastructure.Tests.Integration.Common.Fixtures;

using MediatR;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace Infrastructure.Tests.Integration.BusinessLogic.Commands.AbstractBase;

[Collection(nameof(DatabaseFixture))]
public abstract class CommandsTestsBase : FuturesDataFakersClass, IAsyncLifetime
{
    protected readonly IMediator Mediator;
    protected readonly Func<Task> ClearDatabaseAsyncFunc;

    private readonly FuturesTradingDbContextFactory DbContextFactory;
    protected FuturesTradingDbContext ArrangeAssertDbContext;


    public CommandsTestsBase(DatabaseFixture databaseFixture)
    {
        var serviceProvider = this.BuildServiceProvider(databaseFixture.ConnectionString);
        this.Mediator = serviceProvider.GetRequiredService<IMediator>();

        this.ClearDatabaseAsyncFunc = databaseFixture.ClearDatabaseAsync;


        this.DbContextFactory = databaseFixture.DbContextFactory;
        this.ArrangeAssertDbContext = this.DbContextFactory.CreateNoTrackingContext();
    }
    private ServiceProvider BuildServiceProvider(string connectionString)
    {
        var services = new ServiceCollection();
        
        var configuration = new ConfigurationManager();
        configuration.AddJsonFile("appsettings.test.json", optional: false);
        configuration["ConnectionStrings:TradingHistoryDB"] = connectionString;

        services.AddServices(configuration);
        return services.BuildServiceProvider();
    }


    public async Task InitializeAsync() => await Task.CompletedTask;
    public async Task DisposeAsync() => await this.ClearDatabaseAsyncFunc.Invoke();
}
