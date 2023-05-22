using Application.Interfaces.Services.DataAccess.Repositories;

using CryptoAutopilot.Api.Services.Interfaces;

using Infrastructure.Services.DataAccess.Repositories;

using Microsoft.Extensions.DependencyInjection;

using Presentation.Api.Tests.Integration.Common;

using Xunit;

namespace Presentation.Api.Tests.Integration.GeneralEndpointsTests.Base;

public abstract class GeneralEndpointsTestsBase : FakersClass, IClassFixture<DatabaseFixture>, IClassFixture<ApiFactory>
{
    protected readonly ApiFactory ApiFactory;
    protected readonly HttpClient HttpClient;

    protected IFuturesOrdersRepository OrdersRepository;
    protected IStrategiesTracker StrategiesTracker;
    
    protected GeneralEndpointsTestsBase(ApiFactory apiFactory, DatabaseFixture databaseFixture)
    {
        this.ApiFactory = apiFactory;
        this.HttpClient = this.ApiFactory.CreateClient();
        
        this.OrdersRepository = new FuturesOrdersRepository(databaseFixture.DbContextFactory.Create());
        this.StrategiesTracker = this.ApiFactory.Services.GetRequiredService<IStrategiesTracker>();
    }
}
