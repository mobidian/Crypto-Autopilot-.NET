﻿using Application.DataAccess.Services;

using Infrastructure.DataAccess.Repositories;
using Infrastructure.DataAccess.Services;
using Infrastructure.Tests.Integration.DataAccess.Abstract;

using Tests.Integration.Common.Fixtures;

namespace Infrastructure.Tests.Integration.DataAccess.FuturesOperationsServiceTests.AbstractBase;

public abstract class FuturesOperationsServiceTestsBase : FuturesRepositoriesTestsBase
{
    protected IFuturesOperationsService SUT;

    protected FuturesOperationsServiceTestsBase(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
        var ctx = this.DbContextFactory.Create();
        var ordersRepository = new FuturesOrdersRepository(ctx);
        var positionsRepository = new FuturesPositionsRepository(ctx);

        this.SUT = new FuturesOperationsService(ctx, positionsRepository, ordersRepository);
    }
}