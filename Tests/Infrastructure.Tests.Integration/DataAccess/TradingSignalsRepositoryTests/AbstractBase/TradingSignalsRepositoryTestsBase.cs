﻿using Application.DataAccess.Repositories;

using Infrastructure.DataAccess.Repositories;
using Infrastructure.Tests.Integration.DataAccess.Abstract;

using Tests.Integration.Common.Fixtures;

namespace Infrastructure.Tests.Integration.DataAccess.TradingSignalsRepositoryTests.AbstractBase;

public abstract class TradingSignalsRepositoryTestsBase : FuturesRepositoriesTestsBase
{
    protected ITradingSignalsRepository SUT;

    protected TradingSignalsRepositoryTestsBase(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
        this.SUT = new TradingSignalsRepository(this.DbContextFactory.Create());
    }
}
