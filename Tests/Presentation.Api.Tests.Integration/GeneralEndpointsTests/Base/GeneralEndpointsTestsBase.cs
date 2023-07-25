using Application.DataAccess.Repositories;
using Application.DataAccess.Services;

using CryptoAutopilot.Api.Services.Interfaces;

using Infrastructure.DataAccess.Repositories;
using Infrastructure.DataAccess.Services;

using Microsoft.Extensions.DependencyInjection;

using Presentation.Api.Tests.Integration.Common;

using Tests.Integration.Common.Fixtures;

using Xunit;

namespace Presentation.Api.Tests.Integration.GeneralEndpointsTests.Base;

[Collection(nameof(DatabaseFixture))]
public abstract class GeneralEndpointsTestsBase : FakersClass, IClassFixture<ApiFactory>
{
    protected readonly ApiFactory ApiFactory;
    protected readonly HttpClient HttpClient;


    protected IFuturesOrdersRepository ArrangeOrdersRepository;
    protected IFuturesPositionsRepository ArrangePositionsRepository;
    protected IFuturesOperationsService ArrangeFuturesOperationsService;

    protected IStrategiesTracker StrategiesTracker;
    

    protected GeneralEndpointsTestsBase(ApiFactory apiFactory, DatabaseFixture databaseFixture)
    {
        this.ApiFactory = apiFactory;
        this.HttpClient = this.ApiFactory.CreateClient();


        var ctx = databaseFixture.DbContextFactory.CreateNoTrackingContext();
        this.ArrangeOrdersRepository = new FuturesOrdersRepository(ctx);
        this.ArrangePositionsRepository = new FuturesPositionsRepository(ctx);
        this.ArrangeFuturesOperationsService = new FuturesOperationsService(ctx, this.ArrangePositionsRepository, this.ArrangeOrdersRepository);

        this.StrategiesTracker = this.ApiFactory.Services.GetRequiredService<IStrategiesTracker>();
    }
}
